namespace FsAICore
open System
open System.Text.Json.Nodes
open System.Reflection
open System.ComponentModel
open System.Collections.Generic
open Microsoft.FSharp.Reflection
open FSharp.Data.JsonSchema
open NJsonSchema

///<summary>
/// F# DU friendly JSON Schema Generation.<br />
/// Extends FSharp.Data.JsonSchema for schema generation.<br />
/// Use FSharp.Data.Json members for serialization and deserialization.<br />
/// For compatible serialization options, use FSharp.Data.Json.DefaultOptions
/// </summary>
module JsonUtils =
    let private casePropertyName = "type"
    let private schemaGenerator = lazy (Generator.CreateMemoized(?casePropertyName = Some casePropertyName))
    let private bindingFlags = BindingFlags.Public ||| BindingFlags.Instance

    let private tryGetEnumDescriptions (enumType:Type) =
        let names = Enum.GetNames(enumType)
        let descriptionLookup =
            enumType.GetFields(BindingFlags.Public ||| BindingFlags.Static)
            |> Array.choose (fun field ->
                if field.IsSpecialName then None
                else
                    match field.GetCustomAttribute<DescriptionAttribute>() with
                    | null -> None
                    | attr when String.IsNullOrWhiteSpace attr.Description -> None
                    | attr -> Some(field.Name, attr.Description))
            |> Map.ofArray
        if names |> Array.forall descriptionLookup.ContainsKey then
            names |> Array.map (fun name -> descriptionLookup.[name]) |> Some
        else
            None

    let rec private collectEnumCandidates (visited:HashSet<Type>) (t:Type) =
        if isNull t then ()
        else
            let underlying =
                if t.IsGenericType && t.GetGenericTypeDefinition() = typedefof<Nullable<_>> then
                    t.GetGenericArguments().[0]
                else t
            if isNull underlying then ()
            elif visited.Contains(underlying) then ()
            else
                visited.Add(underlying) |> ignore
                if underlying.IsEnum then ()
                elif underlying = typeof<string> || underlying = typeof<decimal> || underlying = typeof<DateTime> || underlying = typeof<DateTimeOffset> || underlying = typeof<TimeSpan> || underlying.IsPrimitive then
                    ()
                else
                    if underlying.IsArray then
                        collectEnumCandidates visited (underlying.GetElementType())

                    if underlying.IsGenericType then
                        underlying.GetGenericArguments()
                        |> Array.iter (collectEnumCandidates visited)

                    if FSharpType.IsUnion(underlying, true) then
                        FSharpType.GetUnionCases(underlying, true)
                        |> Array.iter (fun caseInfo ->
                            caseInfo.GetFields()
                            |> Array.iter (fun field -> collectEnumCandidates visited field.PropertyType))
                    elif FSharpType.IsRecord(underlying, true) || (underlying.IsClass && underlying <> typeof<string>) || (underlying.IsValueType && not underlying.IsPrimitive && not underlying.IsEnum) then
                        underlying.GetProperties(bindingFlags)
                        |> Array.filter (fun prop -> prop.GetIndexParameters().Length = 0 && prop.CanRead)
                        |> Array.iter (fun prop -> collectEnumCandidates visited prop.PropertyType)

                        underlying.GetFields(bindingFlags)
                        |> Array.filter (fun field -> not field.IsSpecialName)
                        |> Array.iter (fun field -> collectEnumCandidates visited field.FieldType)

    let private buildEnumDescriptionMap (rootType:Type) =
        let visited = HashSet<Type>()
        collectEnumCandidates visited rootType
        visited
        |> Seq.filter (fun ty -> ty.IsEnum)
        |> Seq.choose (fun enumType ->
            tryGetEnumDescriptions enumType
            |> Option.map (fun descriptions -> enumType.Name, descriptions))
        |> Map.ofSeq

    let private ensureExtensionData (schema:JsonSchema) =
        if isNull schema.ExtensionData then
            schema.ExtensionData <- Dictionary<string, obj>()
        schema.ExtensionData

    let private applyEnumDescriptions (schema:JsonSchema) (enumDescriptions:Map<string, string array>) (rootType:Type) =
        let tryAttach key target =
            match Map.tryFind key enumDescriptions with
            | Some descriptions ->
                ensureExtensionData target
                |> fun extensions -> extensions["enumDescriptions"] <- descriptions
            | None -> ()

        if schema.IsEnumeration then
            let name = if String.IsNullOrWhiteSpace schema.Title then rootType.Name else schema.Title
            tryAttach name schema

        if not (isNull schema.Definitions) then
            for KeyValue(name, defSchema) in schema.Definitions do
                if defSchema.IsEnumeration then
                    tryAttach name defSchema
        schema

    let rec private stripEnumNameExtensions (node:JsonNode) =
        match node with
        | :? JsonObject as obj ->
            obj.Remove("x-enumNames") |> ignore
            for KeyValue(_, value) in obj do
                stripEnumNameExtensions value
        | :? JsonArray as arr ->
            for item in arr do
                stripEnumNameExtensions item
        | _ -> ()

    let toSchema (t:Type) : JsonNode =
        let jsonSchema = schemaGenerator.Value t
        let enrichedSchema =
            let enumDescriptions = buildEnumDescriptionMap t
            if Map.isEmpty enumDescriptions then jsonSchema
            else applyEnumDescriptions jsonSchema enumDescriptions t
        let jsonNode = enrichedSchema.ToJson() |> JsonNode.Parse
        stripEnumNameExtensions jsonNode
        jsonNode

    