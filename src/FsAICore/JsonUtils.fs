namespace FsAICore
open System
open System.Text.Json
open System.Text.Json.Nodes
open System.Reflection
open System.ComponentModel
open System.Collections.Generic
open Microsoft.Extensions.AI
open Microsoft.FSharp.Reflection

///<summary>
/// F# DU friendly JSON Schema Generation.<br />
/// Uses Microsoft.Extensions.AI for schema generation.
/// </summary>
module JsonUtils =
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

    let private tryGetStringProperty (propertyName:string) (obj:JsonObject) =
        let mutable node: JsonNode = null
        if obj.TryGetPropertyValue(propertyName, &node) then
            match node with
            | :? JsonValue as value ->
                match value.GetValueKind() with
                | JsonValueKind.String -> value.GetValue<string>() |> Some
                | _ -> None
            | _ -> None
        else
            None

    let private addEnumDescriptions (obj:JsonObject) (descriptions:string array) =
        let values = JsonArray()
        descriptions
        |> Array.iter (fun description -> values.Add(JsonValue.Create description))
        obj["enumDescriptions"] <- values

    let private applyEnumDescriptions (rootType:Type) (enumDescriptions:Map<string, string array>) (node:JsonNode) =
        let rec visit currentName (node:JsonNode) =
            match node with
            | :? JsonObject as obj ->
                let schemaName =
                    tryGetStringProperty "title" obj
                    |> Option.orElse currentName

                if obj.ContainsKey "enum" then
                    [ schemaName; currentName; Some rootType.Name ]
                    |> List.choose id
                    |> List.tryPick (fun name -> Map.tryFind name enumDescriptions)
                    |> Option.iter (addEnumDescriptions obj)

                obj
                |> Seq.toArray
                |> Array.iter (fun (KeyValue(name, value)) -> visit (Some name) value)
            | :? JsonArray as arr ->
                arr
                |> Seq.toArray
                |> Array.iter (visit currentName)
            | _ -> ()
        visit None node

    let rec private stripEnumNameExtensions (node:JsonNode) =
        if isNull node then ()
        else
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
        let jsonNode =
            AIJsonUtilities.CreateJsonSchema(t).GetRawText()
            |> JsonNode.Parse

        let enumDescriptions = buildEnumDescriptionMap t
        if not (Map.isEmpty enumDescriptions) then
            applyEnumDescriptions t enumDescriptions jsonNode

        stripEnumNameExtensions jsonNode
        jsonNode

    
