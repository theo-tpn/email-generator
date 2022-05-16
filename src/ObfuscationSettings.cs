using System.Reflection;

[assembly: Obfuscation(Feature = "apply to type *: apply to member * when method or constructor: virtualization", Exclude = false)]
[assembly: Obfuscation(Feature = "encrypt symbol names with password 147852369", Exclude = false)]
[assembly: Obfuscation(Feature = "ildasm suppression", Exclude = true)]
[assembly: Obfuscation(Feature = "rename symbol names with printable characters", Exclude = false)]
[assembly: Obfuscation(Feature = "type renaming pattern 'b'.*", Exclude = false)]

namespace Noctus.Common
{
    public class ObfuscationSettings
    {
    }
}
