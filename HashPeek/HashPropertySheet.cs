using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using SharpShell.Attributes;
using SharpShell.SharpPropertySheet;

namespace HashPeek;

[ComVisible(true)]
[Guid("e2c4a1f7-5b8d-4a3e-9c60-7d1f2b6a8c34")]
[COMServerAssociation(AssociationType.AllFiles)]
public sealed class HashPropertySheet : SharpPropertySheet
{
    protected override bool CanShowSheet()
    {
        var paths = SelectedItemPaths.Take(2).ToList();
        return paths.Count == 1 && File.Exists(paths[0]);
    }

    protected override IEnumerable<SharpPropertyPage> CreatePages()
    {
        return new SharpPropertyPage[] { new HashPropertyPage() };
    }
}
