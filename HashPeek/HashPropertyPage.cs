using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpShell.SharpPropertySheet;

namespace HashPeek;

public sealed class HashPropertyPage : SharpPropertyPage
{
    private readonly Dictionary<string, TextBox> hashBoxes = new Dictionary<string, TextBox>();
    private readonly Label statusLabel;
    private bool closed;

    public HashPropertyPage()
    {
        PageTitle = "Hashes";

        var table = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 6,
            Padding = new Padding(12)
        };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        for (var row = 0; row < table.RowCount; row++)
        {
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        }

        statusLabel = new Label { Text = "Ready", AutoSize = true, Margin = new Padding(0, 0, 0, 10) };
        table.Controls.Add(statusLabel, 0, 0);
        table.SetColumnSpan(statusLabel, 3);

        var names = new[] { "SHA-256", "SHA-512", "MD5", "CRC32", "xxHash64" };
        for (var i = 0; i < names.Length; i++)
        {
            var row = i + 1;
            var name = names[i];
            table.Controls.Add(new Label { Text = name, AutoSize = true, Anchor = AnchorStyles.Left }, 0, row);

            var valueBox = new TextBox { ReadOnly = true, Dock = DockStyle.Fill, Text = "..." };
            table.Controls.Add(valueBox, 1, row);
            hashBoxes.Add(name, valueBox);

            var copyButton = new Button { Text = "Copy", AutoSize = true, Anchor = AnchorStyles.Right };
            copyButton.Click += (_, _) =>
            {
                if (valueBox.Text.Length > 0 && valueBox.Text != "...")
                {
                    Clipboard.SetText(valueBox.Text);
                }
            };
            table.Controls.Add(copyButton, 2, row);
        }

        Controls.Add(table);
    }

    protected override void OnPropertyPageInitialised(SharpPropertySheet parent)
    {
        var path = parent.SelectedItemPaths.First();
        _ = CalculateAsync(path);
    }

    protected override void OnPropertySheetClose()
    {
        closed = true;
        base.OnPropertySheetClose();
    }

    private async Task CalculateAsync(string path)
    {
        OnUi(() => statusLabel.Text = "Calculating...");
        try
        {
            var progress = new Progress<int>(percent => OnUi(() =>
            {
                if (!closed)
                {
                    statusLabel.Text = $"Calculating... {percent}%";
                }
            }));
            var results = await Task.Run(() => HashCalculator.Compute(path, progress));

            OnUi(() =>
            {
                if (closed)
                {
                    return;
                }

                foreach (var result in results)
                {
                    hashBoxes[result.Name].Text = result.Value;
                }

                statusLabel.Text = "Done";
            });
        }
        catch (Exception ex)
        {
            OnUi(() =>
            {
                if (!closed)
                {
                    statusLabel.Text = "Failed: " + ex.Message;
                }
            });
        }
    }

    private void OnUi(Action action)
    {
        if (IsHandleCreated && InvokeRequired)
        {
            BeginInvoke(action);
        }
        else
        {
            action();
        }
    }
}
