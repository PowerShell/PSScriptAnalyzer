function TrapTest {
    trap {"Error found: $_"}
    nonsensString
}