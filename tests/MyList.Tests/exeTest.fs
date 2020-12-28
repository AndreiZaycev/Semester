module exeTest
open Expecto
open Listik
open Tree

[<Tests>]
let firstPackTest =
    testList "Check equatation"
        [
            testProperty "checks equivivalent list and MyList"
            <| fun (k: int) ->
                if abs k >= 1
                then
                    let myList = generator (abs k)
                    Expect.equal (toMyList (toDefoltList myList)) myList "needs to be equal"

            testProperty "checks equivivalent list and MyList #2"
            <| fun (k: int) ->
                if abs k >= 1
                then
                    let commonList = List.init (abs k) (fun _ -> System.Random().Next())
                    Expect.equal (toDefoltList (toMyList commonList)) commonList "needs to be equal"

            testProperty "Check length id"
            <| fun (k: int) ->
                if abs k >= 1
                then
                    let myList = generator (abs k)
                    Expect.equal ((toDefoltList myList).Length) (length myList) "length need to be equal"

            testProperty "Check concat id"
            <| fun (k: int,t: int) ->
                if abs k >= 1 && abs t >= 1 
                then
                    let myList0 = generator (abs t)
                    let myList = generator (abs k)
                    Expect.equal (toMyList (List.concat [(toDefoltList myList0); (toDefoltList myList)])) (concat myList0 myList) "equality needs to be"

            testProperty "Check sort id"
            <| fun (k: int) ->
                if abs k >= 1
                then
                    let myList = generator (abs k)
                    Expect.equal ((List.sort (toDefoltList myList))) (toDefoltList (sort myList)) "needs to be equal"

            testProperty "Check iter id"
            <| fun (k: int) ->
                if abs k >= 1
                then
                    let myList = generator (abs k)
                    let mutable acc = 0
                    let mutable acc1 = 0
                    List.iter (fun elem -> acc <- acc + elem) (toDefoltList myList)
                    iter (fun elem -> acc1 <- acc1 + elem) myList
                    Expect.equal acc acc1 "needs to be eqaul"

            testProperty "Check map id"
            <| fun (k: int) ->
                if abs k >= 1
                then
                    let genList = generator (abs k)
                    let myList = map ((+) 2) genList
                    let defList = List.map ((+) 2) (toDefoltList genList)                  
                    Expect.equal myList (toMyList defList) "needs to be equal"

            testProperty "Check toMyString and toString id "
            <| fun (str: string) ->
                if not (System.String.IsNullOrEmpty str) 
                then                    
                    Expect.equal (toString (toMyString str)) str "needs to be equal"
        ]
[<Tests>]
let secondTest =
    testList "testcases"
        [

            testCase "checks equivivalent list and MyList1"
            <| fun _ ->
                let myList = Cons (5, Cons (4, Cons (3, Cons (2, Cons (1, One 0)))))
                Expect.equal (toMyList (toDefoltList myList)) (Cons (5, Cons (4, Cons (3, Cons (2, Cons (1, One 0)))))) "needs to be equal"

            testCase "checks equivivalent list and MyList2"
            <| fun _ ->
                let lst = [1;2;3;4;5;6;7]
                Expect.equal (toDefoltList (toMyList lst)) [1;2;3;4;5;6;7] "needs to be equal"

            testCase "check length"
            <| fun _ ->
                let lst = Cons (5, Cons (4, Cons (3, Cons (2, Cons (1, One 0)))))
                Expect.equal (length lst) 6 "needs to be equal"

            testCase "check sort"
                <| fun _ ->
                    let myList = Cons (5, Cons (4, Cons (3, Cons (2, Cons (1, One 0)))))
                    Expect.equal (sort myList) (Cons (0, Cons (1, Cons (2, Cons (3, Cons (4, One 5)))))) "needs to be equal"

            testCase "stringToMyString"
            <| fun _ ->
                Expect.equal "привет" (toString (toMyString "привет")) "need to be equal"

            testCase "string concat1"
            <| fun _ ->
                Expect.equal ("привет" + " Андрей") (toString (concatMyString (toMyString "привет") (toMyString " Андрей"))) "Needs to be equal"

            testCase "string concat2"
            <| fun _ ->
                Expect.equal (" " + " ") (toString (concatMyString (toMyString " ") (toMyString " "))) "Needs to be equal"

            testCase "avgTree1"
            <| fun _ ->
                Expect.equal (average (Node ((15), Cons ((Leaf (13)), One (Leaf (10)))))) 12 "38 / 3 = 12"

            testCase "avgTree2"
            <| fun _ ->
                Expect.equal (average (Node ((1), Cons ((Leaf (2)), One (Leaf (3)))))) 2 "6 / 3 = 2"

            testCase "maxInTree1"
            <| fun _ ->
                Expect.equal (max (Node ((15), Cons ((Leaf (13)), One (Leaf (10)))))) 15 "max is 15 in tree"

            testCase "maxInTree2"
            <| fun _ ->
                Expect.equal (max (Node ((-100), Cons ((Leaf (-13)), One (Leaf (-10)))))) -10 "max is -10 in tree"
           
            testCase "map"
            <| fun _ ->
                let myList = generator 5
                Expect.equal (List.map ((+) 2) (toDefoltList myList)) (toDefoltList (map ((+) 2) myList)) "needs to be equal"

            testCase "map1"
            <| fun _ ->
                let myList = generator 5
                Expect.equal (List.map ((*) 2) (toDefoltList myList)) (toDefoltList (map ((*) 2) myList)) "needs to be equal" 
   
            testCase "iter1"
            <| fun _ ->
                let myList = generator 5
                let mutable acc = 0
                let mutable acc1 = 0
                List.iter (fun elem -> acc <- acc + elem) (toDefoltList myList)
                iter (fun elem -> acc1 <- acc1 + elem) myList
                Expect.equal acc acc1 "needs to be equal"

            testCase "iter"
            <| fun _ ->
                let myList = generator 5
                let mutable acc = 0
                let mutable acc1 = 0
                List.iter (fun elem -> acc <- acc - elem) (toDefoltList myList)
                iter (fun elem -> acc1 <- acc1 - elem) myList
                Expect.equal acc acc1 "needs to be equal"
        ]
