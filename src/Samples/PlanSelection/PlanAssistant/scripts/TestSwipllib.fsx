#r "/Users/faisalwaris/repos/RTOpenAI/SwiplLib/bin/Debug/net9.0/SwiplLib.dll"
open System
open FSharp.NativeInterop
open System.Runtime.InteropServices
open Swipl
open type Swipl.Methods
open System.IO


#nowarn "9"
let TRUE = 1 //
let FALSE = 0
type Term = TVar | TAtom of string | TInt of int | TFloat of float | TStr of string | TTerm of (string*int)
    with member x.AsString() =
            match x with
            | TVar -> "var"
            | TAtom s -> s
            | TStr s -> s
            | TFloat f -> string f
            | TInt i -> string i
            | TTerm (t,a) -> $"{t}/{a}"

let getAtomChars(term_t:unativeint) =
    // Allocate unmanaged memory to hold one pointer
    let bufferPtr = Marshal.AllocHGlobal(IntPtr.Size)
    try
        // Convert the allocated IntPtr to a native pointer
        let nativeBuffer : nativeptr<nativeptr<sbyte>> = NativePtr.ofNativeInt (bufferPtr.ToInt64() |> nativeint)
        // Call the C function. It is expected to write a pointer to a null-terminated ANSI string.
        let r = PL_get_atom_chars(term_t,nativeBuffer)
        // Read the native pointer (of type nativeptr<sbyte>) from our allocated memory
        let sbytePtr = NativePtr.read nativeBuffer
        // Convert the native pointer to a nativeint and then to an IntPtr for marshaling
        let charPtr = NativePtr.toNativeInt sbytePtr
        // Convert the null-terminated ANSI string at charPtr into an F# string.
        Marshal.PtrToStringAnsi(charPtr)
    finally
        // Always free the allocated memory.
        Marshal.FreeHGlobal(bufferPtr)

let getAnsiString (term_t :unativeint)  =
    // Allocate unmanaged memory to hold one pointer
    let bufferPtr = Marshal.AllocHGlobal(IntPtr.Size)
    let lenPtr = Marshal.AllocHGlobal(Marshal.SizeOf<unativeint>())
    try
        // Convert the allocated IntPtr to a native pointer
        let nativeBuffer : nativeptr<nativeptr<sbyte>> = NativePtr.ofNativeInt (bufferPtr.ToInt64() |> nativeint)
        let nativeLenPtr = NativePtr.ofNativeInt<unativeint> (lenPtr.ToInt64() |> nativeint)
        // Call the C function. It is expected to write a pointer to a null-terminated ANSI string.
        let r = PL_get_string(term_t,nativeBuffer,nativeLenPtr)
        // Read the native pointer (of type nativeptr<sbyte>) from our allocated memory
        let sbytePtr = NativePtr.read nativeBuffer
        // Convert the native pointer to a nativeint and then to an IntPtr for marshaling
        let charPtr = NativePtr.toNativeInt sbytePtr
        // Convert the null-terminated ANSI string at charPtr into an F# string.
        Marshal.PtrToStringAnsi(charPtr)
    finally
        // Always free the allocated memory.
        Marshal.FreeHGlobal(bufferPtr)
        Marshal.FreeHGlobal lenPtr

let getFloat (term_t:unativeint) =
    let floatPtr = NativePtr.stackalloc<float> 1
    let r = PL_get_float(term_t,floatPtr)
    NativePtr.get floatPtr 0
    
let getInt (term_t:unativeint) =
    let intPtr = NativePtr.stackalloc<int> 1
    let r = PL_get_integer(term_t,intPtr)
    NativePtr.get intPtr 0
   
let rec getTermValue (term_t:unativeint) =
   let mutable termValue = Unchecked.defaultof<term_value_t>
   let termValuePtr = &&termValue
   let x = PL_get_term_value(term_t,termValuePtr)
   let arity = int termValue.t.arity
   let atom = getAtomChars(termValue.t.name)   
   atom,arity
    
and getTerm (term_t : unativeint ) =
    let v = PL_term_type(term_t)   
    match enum<Swipl.TermType>(v) with
    | Swipl.TermType.PL_VARIABLE -> TVar
    | Swipl.TermType.PL_ATOM  -> TAtom (getAtomChars term_t)
    | Swipl.TermType.PL_STRING -> TStr (getAnsiString term_t)
    | Swipl.TermType.PL_FLOAT -> TFloat (getFloat term_t)
    | Swipl.TermType.PL_INTEGER -> TInt (getInt term_t)
    | Swipl.TermType.PL_TERM -> TTerm (getTermValue(term_t))
    | x -> failwith $"term type of {x} not handled"
    
and check (ret:int) =
    if ret = FALSE then
        let term_t = PL_exception(NativePtr.nullPtr)
        if term_t = unativeint 0 then 
            failwith "unknown error"
        else
            let t = getTerm term_t
            failwith (t.AsString())           

// A helper function that opens a query and collects all solution values.
// Here we assume the predicate "my_predicate/1" returns an integer.
let getAllSolutions (predicateName: string) (arity: int) =
    let predicate = Methods.PL_predicate(predicateName, arity, "user")
    // Create a new term reference that will hold the query term.
    let term = Methods.PL_new_term_ref()
    // Open the query.
    let qid = Methods.PL_open_query(NativePtr.nullPtr, 0, predicate, term)
    // Recursively collect each solution.
    let rec loop acc =
        if Methods.PL_next_solution(qid) <> 0 then
            
            let value = getTerm term
            loop (value :: acc)
        else
            acc
    let sols = loop []
    // Close the query.
    Methods.PL_close_query(qid) |> ignore
    List.rev sols
    
let clausesFile = __SOURCE_DIRECTORY__ + "/../Resources/Raw/wwwroot/plan_clauses.pl"

let loadPrologFile (fileName: string) =
    let file = Path.GetFullPath(fileName)
    if not (File.Exists fileName) then failwith $"Does not exist '{fileName}'"
    let term_t = PL_new_term_ref()
    let s = $"consult('{fileName}')."    
    PL_chars_to_term(s,term_t) |> check
    PL_call(term_t,NativePtr.nullPtr) |> check
    
let getVars(exp:string) =
    let pl = $"p(Vars) :- read_term_from_chars('{exp}'),_,[variable_names(Vars)])."
    let pl = """p(Term,Vars) :- read_term_from_chars("plan(T,_,_,_).", Term, [variable_names(Vars)])."""

    let term = PL_new_term_ref()
    PL_chars_to_term(pl,term) |> check
    PL_assert(term,NativePtr.nullPtr,0) |> check
    let vars = getAllSolutions "p" 2    
    vars |> List.iter (printfn "%A")
              
let run() =    
    PL_initialise(2,[|"libswipl";"-q"|]) |> check
    loadPrologFile clausesFile
    // Convert the clause string into a Prolog term.
    let exp = "plant(T,_,_,_)."
    getVars exp
    //
    // let clause = PL_new_term_ref()    
    // //let clauseStr = "p(X) :- X = 42."
    // let clauseStr = "p(T) :- plan(T,_,_,_)."    
    // PL_chars_to_term(clauseStr, clause) |> check
    // PL_assert(clause,NativePtr.nullPtr,0) |> check
    // let sols = getAllSolutions "p" 1
    // sols |> List.iter (printfn "%A")
    PL_cleanup(0) |> ignore
    //PL_halt(1) //seems to exit process
    
(*
run()
*)
    

