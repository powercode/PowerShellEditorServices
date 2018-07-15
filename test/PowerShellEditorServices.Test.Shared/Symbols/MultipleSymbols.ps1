$Global:GlobalVar = 0
$UnqualifiedScriptVar = 1
$Script:ScriptVar2 = 2

"`$Script:ScriptVar2 is $Script:ScriptVar2"

function AFunction {}

filter AFilter {$_}

function AnAdvancedFunction {
    begin {
        $LocalVar = 'LocalVar'
        function ANestedFunction() {
            $nestedVar = 42
            "`$nestedVar is $nestedVar"
        }
    }
    process {
        ANestedFunction
    }
    end {}
}

workflow AWorkflow {}

Configuration AConfiguration {
    Node "TEST-PC" {}
}


AFunction
1..3 | AFilter
AnAdvancedFunction

class AClass {
    static [int] $AStaticProperty
    [int] $AProperty

    AClass() {
        $this.AProperty = -1
    }

    AClass([int] $value) {
        $this.AProperty = $value
    }

    [int] AMethod() { return $this.AProperty }
    [int] AMethod([string] $overloadedParameter) { return $this.AProperty }
}

[AClass]::new()

[AClass]::AStaticProperty

$a.AMethod()

$a.AMethod("str")
