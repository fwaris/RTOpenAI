/*
append(List,[],List).
append([Item|Result],[Item|List1],List2) :-
    append(Result,List1,List2).
*/

appendItem(ResultList,List,Item) :-
    append(ResultList,List,[Item]).

join(List,Left,Pivot,Right) :-
    appendItem(L,Left,Pivot),
    append(List,L,Right).

/*
member(Item,List) :-
    split(L,Item,R,List).
*/

permute([Item],[Item]).
permute(Result,[Item|Items]) :-
    permute(P,Items),
    append(P,Left,Right),
    appendItem(R,Left,Item),
    append(Result,R,Right).

size(0,[]).
size(Size,[Item|Items]) :-
    size(S,Items),
    (Size := (S + 1)).

split(Left,Pivot,Right,List) :-
    append(List,L,Right),
    appendItem(L,Left,Pivot).

/*
reverse([],[]).
reverse(Result,[Item|Items]) :-
    reverse(R,Items),
    appendItem(Result,R,Item).
*/

prefix(Prefix,Items) :-
    append(Items,Prefix,L).

suffix(Suffix,Items) :-
    append(Items,L,Suffix).


partition([],[],Pivot,[]).
partition([Item|LessEqual],Greater,Pivot,[Item|Items]) :-
    (Item =< Pivot),
    partition(LessEqual,Greater,Pivot,Items).
partition(LessEqual,[Item|Greater],Pivot,[Item|Items]) :-
    (Item > Pivot),
    partition(LessEqual,Greater,Pivot,Items).

qsort([],[]).
qsort(Result,[Item|Items]) :-
    partition(LE,G,Item,Items),
    qsort(SortedLE,LE),
    qsort(SortedG,G),
    join(Result,SortedLE,Item,SortedG).

sequence(Items,Size) :-
    sequence(Items,1,Size).

divide([],Items,Items,0).
divide([Item|Left],Right,[Item|Items],Count) :-
    greater(Count,0),
    (SubCount is subtract(Count,1)),
    divide(Left,Right,Items,SubCount).

shuffle(Items,Items,0).
shuffle(Result,Items,Count) :-
    greater(Count,0),
    (SubCount is subtract(Count,1)),
    shuffle(S,Items,SubCount),
    size(Size,Items),
    random(0,Size,R),
    divide(Left,Right,S,R),
    merge(Result,Right,Left).

merge([LeftItem,RightItem|Items],[LeftItem|Left],[RightItem|Right]) :-
    merge(Items,Left,Right).
merge([],[],[]).
merge(Items,Items,[]) :-
    size(Size,Items),
    greater(Size,0).
merge(Items,[],Items) :-
    size(Size,Items),
    greater(Size,0).

sequence([Min|Items],Min,Max) :-
    less(Min,Max),
    (MinPlus is add(Min,1)),
    sequence(Items,MinPlus,Max).
sequence([MinMax],MinMax,MinMax).
