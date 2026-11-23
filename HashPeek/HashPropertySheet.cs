using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using SharpShell.Attributes;
using SharpShell.SharpPropertySheet;

namespace HashPeek;

[ComVisible(true)]
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
