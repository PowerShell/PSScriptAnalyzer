[string]::Empty
  
1..10 | %{ $x."Prop$_" }
"abc".("This is no error")