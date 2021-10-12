module Message

type BalancerMessage =
    | EOS of AsyncReplyChannel<unit>
    | NamedPair of ((int[,] * string) * (int[,] * string))

type LoaderMessage = 
    | EOS of AsyncReplyChannel<unit>
    | Continue of AsyncReplyChannel<unit>