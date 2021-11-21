namespace Logic

module Cos =
    type Disc =
        Raz of int
        | Dwa of string
        
    type Rek = {
        Trzy: int
        Cztery: string option
    }
    
    let fu (d:Disc) (r:Rek) =
        match d, r with
        | Raz a, _ -> r.Trzy.ToString()
        | Dwa b, {Rek.Trzy = _; Rek.Cztery = Some c} -> c
        | Dwa b, {Rek.Trzy = _; Rek.Cztery = None} -> "nie znaleziono"