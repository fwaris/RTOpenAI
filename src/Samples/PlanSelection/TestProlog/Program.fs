module Program
open System
open System.IO
open Prolog


let e4 = PrologEngine(persistentCommandHistory=false)
e4.Consult(@"/Users/faisalwaris/repos/RTOpenAI/src/RTOpenAI/scripts/o1_plan_clauses.pl")
e4.Consult(@"/Users/faisalwaris/repos/RTOpenAI/src/RTOpenAI/scripts/plan_utils.pl")
//let m = e4.GetFirstSolution("find_all_notes(Xs),print_all(Xs).")
//let m1 = e4.GetFirstSolution("find_all_included_benefits(Xs),print_all(Xs).")
let m2 = e4.GetFirstSolution("find_all_disclaimers(Xs),print_all(Xs).")
let q = """
string_words("this is a cat",D).
"""
//let m2 = e4.GetFirstSolution(q)
printfn $"{m2}"