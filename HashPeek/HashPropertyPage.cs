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
    private readonly ToolTip toolTip = new ToolTip();
    private bool closed;

    public HashPropertyPage()
    {
        PageTitle = "Hashes";

        var names = new[] { "SHA-1", "SHA-256", "SHA-512", "MD5", "CRC32", "xxHash64", "XXH3_64", "BLAKE3" };
        var table = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = names.Length + 1,
            Padding = new Padding(12)
        };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 84));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 28));
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        for (var row = 1; row < table.RowCount; row++)
        {
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
        }

        statusLabel = new Label { Text = "Ready", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
        table.Controls.Add(statusLabel, 0, 0);
        table.SetColumnSpan(statusLabel, 3);

        var copyIcon = CreateCopyIcon();
        for (var i = 0; i < names.Length; i++)
        {
            var row = i + 1;
            var name = names[i];
            table.Controls.Add(new Label { Text = name, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, row);

            var valueBox = new TextBox { ReadOnly = true, Dock = DockStyle.Fill, Text = "..." };
            table.Controls.Add(valueBox, 1, row);
            hashBoxes.Add(name, valueBox);

            var copyButton = new Button
            {
                Image = copyIcon,
                Dock = DockStyle.Fill,
                Margin = new Padding(2, 1, 2, 1),
                FlatStyle = FlatStyle.Flat
            };
            toolTip.SetToolTip(copyButton, "Copy to clipboard");
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

    // Two overlapping outlines: a compact copy glyph that works without extra assets.
    private static Bitmap CreateCopyIcon()
    {
        var icon = new Bitmap(16, 16);
        using var graphics = Graphics.FromImage(icon);
        using var pen = new Pen(SystemColors.ControlText);
        graphics.Clear(Color.Transparent);
        graphics.DrawRectangle(pen, 1, 4, 10, 11);
        graphics.DrawRectangle(pen, 5, 1, 10, 11);
        return icon;
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
