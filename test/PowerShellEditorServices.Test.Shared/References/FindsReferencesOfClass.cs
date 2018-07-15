namespace Microsoft.PowerShell.EditorServices.Test.Shared.References
{
    public class FindsReferencesOfClass
    {
        public static readonly ScriptRegion SourceDetails =
            new ScriptRegion
            {
                File = @"References\ReferenceFileF.ps1",
                StartLineNumber = 1,
                StartColumnNumber = 7
            };
    }

    public class FindsReferencesOfClassRefMethod
    {
        public static readonly ScriptRegion SourceDetails =
            new ScriptRegion
            {
                File = @"References\ReferenceFileF.ps1",
                StartLineNumber = 20,
                StartColumnNumber = 4
            };
    }


    public class FindsReferencesOfClassStaticProperty
    {
        public static readonly ScriptRegion SourceDetails =
            new ScriptRegion
            {
                File = @"References\ReferenceFileF.ps1",
                StartLineNumber = 22,
                StartColumnNumber = 13
            };
    }
    public class FindsReferencesOfClassProperty
    {
        public static readonly ScriptRegion SourceDetails =
            new ScriptRegion
            {
                File = @"References\ReferenceFileF.ps1",
                StartLineNumber = 2,
                StartColumnNumber = 23
            };
    }

    public class FindsReferencesOfClassPropertyRef
    {
        public static readonly ScriptRegion SourceDetails =
            new ScriptRegion
            {
                File = @"References\ReferenceFileF.ps1",
                StartLineNumber = 24,
                StartColumnNumber = 4
            };
    }

    public class FindsReferencesOfClassConstructor
    {
        public static readonly ScriptRegion SourceDetails =
            new ScriptRegion
            {
                File = @"References\ReferenceFileF.ps1",
                StartLineNumber = 26,
                StartColumnNumber = 13
            };
    }
}
