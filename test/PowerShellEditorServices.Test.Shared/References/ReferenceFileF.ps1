class ANewType {
    [string] $Property
    $NoTypeProperty
    static [int] $StaticProperty

    ANewType() {
        $this.Property = "prop"
    }

    ANewType([string] $a) {
        $this.Property = $a
    }


    [void] Method1() {}

    [string] MethodWithReturn([string] $argument) {
        return $argument
    }
}

[ANewType]::new()

$a.Method1()

[ANewType]::StaticProperty

$a.Property

[ANewType]::new("a")
