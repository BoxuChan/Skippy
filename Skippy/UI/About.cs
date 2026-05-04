using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text.RegularExpressions;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Interface.Utility.Raii;

namespace Skippy.UI {
    internal sealed partial class Menu {
        private IDalamudTextureWrap? _image1;
        private IDalamudTextureWrap? _image2;
        
        private bool _images;
        private string[]? _content;
        private bool _loaded;

        private string? _ipcCode;
        private string? _ipcDescription;

        private void UIAbout() {
            if (!_images) {
                _images = true;
                
                _ = LoadRemoteImage("https://boxu.fr/ffxiv/praetorium.gif", w => _image1 = w);
                _ = LoadRemoteImage("https://boxu.fr/ffxiv/gaius.png", w => _image2 = w);
            }

            if (!_loaded) {
                _loaded = true;
                
                try {
                    var path = Path.Combine(_pluginInterface.AssemblyLocation.DirectoryName!, "README.md");
                    
                    if (File.Exists(path)) {
                        _content = File.ReadAllLines(path);
                    }
                } catch { }
            }

            if (_content == null) {
                ImGui.TextWrapped("The contents of README.md couldn't be read.");
                
                return;
            }

            bool codeblock  = false;
            bool inIpc = false;
            int i = 0;

            while (i < _content.Length) {
                var line = _content[i];

                if (line.StartsWith("## IPC") || line.StartsWith("## `IPC`"))
                {
                    inIpc = true;
                } else if (line.StartsWith("## ") && inIpc) {
                    inIpc = false;
                }

                if (line.StartsWith("```")) {
                    if (!codeblock) {
                        codeblock = true;
                        var code = new List<string>();
                        i++;
                        
                        while (i < _content.Length && !_content[i].StartsWith("```")) {
                            code.Add(_content[i]);
                            i++;
                        }
                        
                        codeblock = false;
                        i++;

                        if (inIpc) {
                            // silently consume — already handled by the ### lookahead
                        } else {
                            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.60f, 0.85f, 1.00f, 1f));

                            foreach (var cl in code) {
                                ImGui.TextUnformatted(cl);
                            }
                            ImGui.PopStyleColor();
                        }
                    }
                    
                    continue;
                }

                if (line.StartsWith("[![")) {
                    if (line.Contains("praetorium")) {
                        DrawImage(_image1, "Welcome to The Praetorium");
                    } else if (line.Contains("gaius")) {
                        DrawImage(_image2, "Tell me... For whom do you fight?");
                    }

                    i++; 
                    continue;
                }

                if (line.StartsWith("# ")) {
                    ImGui.Spacing();
                    
                    var height = line[2..];
                    ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.80f, 0.80f, 1.00f, 1f));
                    
                    var width = ImGui.GetContentRegionAvail().X;
                    ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (width - ImGui.CalcTextSize(height).X) * 0.5f);
                    ImGui.TextUnformatted(height);
                    ImGui.PopStyleColor();
                    
                    ImGui.Separator(); 
                    ImGui.Spacing();
                    
                    i++; 
                    continue;
                }

                if (line.StartsWith("## ")) {
                    ImGui.Spacing();
                    
                    SectionHeader(line[3..].Trim('_').Trim('"'));
                    
                    i++; 
                    continue;
                }

                if (line.StartsWith("### ")) {
                    if (inIpc) {
                        _ipcCode = null;
                        _ipcDescription = null;
                        var j = i + 1;
                        
                        while (j < _content.Length) {
                            var ahead = _content[j];
                            
                            if (ahead.StartsWith("#")) {
                                break;
                            }
                            
                            if (ahead.StartsWith("```")) {
                                var code = new List<string>();
                                j++;
                                
                                while (j < _content.Length && !_content[j].StartsWith("```")) {
                                    code.Add(_content[j]);
                                    j++;
                                }

                                j++;
                                _ipcCode = string.Join("\n", code);
                            } else if (!string.IsNullOrWhiteSpace(ahead)) {
                                if (_ipcDescription == null) {
                                    _ipcDescription = StripInlineMarkdown(ahead);
                                }

                                j++;
                            } else {
                                j++;
                            }

                            if (_ipcDescription != null && _ipcCode != null) {
                                break;
                            }
                        }

                        RenderIpcHeader(line[4..]);
                        i = j;
                    } else {
                        ImGui.Spacing();
                        
                        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.70f, 0.70f, 0.95f, 1f));
                        ImGui.TextUnformatted(StripInlineMarkdown(line[4..]));
                        ImGui.PopStyleColor();
                        
                        ImGui.Spacing();
                    }
                    
                    i++; 
                    continue;
                }

                if (line.StartsWith("|")) {
                    var tableContent = new List<string>();
                    
                    while (i < _content.Length && _content[i].StartsWith("|")) {
                        tableContent.Add(_content[i]);
                        i++;
                    }
                    
                    RenderMarkdownTable(tableContent);
                    continue;
                }

                if (inIpc && !string.IsNullOrWhiteSpace(line) && !line.StartsWith("#")) {
                    i++; 
                    continue;
                }

                if (inIpc && string.IsNullOrWhiteSpace(line)) {
                    i++;
                    continue;
                }

                if (line.StartsWith("- ") || line.StartsWith("* ")) {
                    ImGui.Bullet(); 
                    ImGui.SameLine();
                    ImGui.TextWrapped(StripInlineMarkdown(line[2..]));
                    
                    i++; 
                    continue;
                }

                if (string.IsNullOrWhiteSpace(line)) {
                    ImGui.Spacing();
                    
                    i++; 
                    continue;
                }

                ImGui.TextWrapped(StripInlineMarkdown(line));
                i++;
            }
        }
        
        private void RenderIpcHeader(string line) {
            var regex = Regex.Matches(line, @"`([^`]+)`");
            
            if (regex.Count < 2) {
                ImGui.TextWrapped(StripInlineMarkdown(line));
                return;
            }
            
            var channel = regex[0].Groups[1].Value;
            var returnType = regex[1].Groups[1].Value;
            
            ImGui.Spacing();
            
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.60f, 0.85f, 1.00f, 1f));
            ImGui.TextUnformatted(channel);
            ImGui.PopStyleColor();
            ImGui.SameLine(0, 4f);

            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.55f, 0.55f, 0.55f, 1f));
            ImGui.TextUnformatted($"-> {returnType}");
            ImGui.PopStyleColor();
            ImGui.SameLine(0, 8f);

            ImGuiComponents.HelpMarker(_ipcDescription ?? $"{channel} -> {returnType}");

            if (_ipcCode != null) {
                ImGui.SameLine(0, 10f);
                ImGui.PushFont(UiBuilder.IconFont);
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.55f, 0.55f, 0.55f, 1f));
                ImGui.TextUnformatted(FontAwesomeIcon.Code.ToIconString());
                ImGui.PopStyleColor();
                ImGui.PopFont();
                
                if (ImGui.IsItemHovered()) {
                    using var tooltip = ImRaii.Tooltip();
                    ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.60f, 0.85f, 1.00f, 1f));
                    ImGui.TextUnformatted(_ipcCode);
                    ImGui.PopStyleColor();
                }

                if (ImGui.IsItemClicked())
                    ImGui.SetClipboardText(_ipcCode);
                
                _ipcCode = null;
            }

            ImGui.Spacing();
        }

        private static void RenderMarkdownTable(List<string> rows) {
            if (rows.Count < 2) {
                return;
            }

            var count = ParseTableRow(rows[0]).Count;

            if (count == 0) {
                return;
            }

            ImGui.Spacing();

            if (!ImGui.BeginTable("##table", count, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingStretchProp)) {
                return;
            }

            for (int c = 0; c < count; c++) {
                ImGui.TableSetupColumn(ParseTableRow(rows[0])[c]);
            }
                
            ImGui.TableHeadersRow();

            for (int r = 2; r < rows.Count; r++) {
                var cells = ParseTableRow(rows[r]);
                ImGui.TableNextRow();
                
                for (int c = 0; c < count && c < cells.Count; c++) {
                    ImGui.TableNextColumn();
                    ImGui.TextWrapped(StripInlineMarkdown(cells[c]));
                }
            }

            ImGui.EndTable();
            ImGui.Spacing();
        }

        private static List<string> ParseTableRow(string row) {
            var cells = new List<string>();

            foreach (var p in row.Split('|', StringSplitOptions.RemoveEmptyEntries)) {
                cells.Add(p.Trim());
            }
            
            return cells;
        }

        private static string StripInlineMarkdown(string text) {
            string regex;
            regex = Regex.Replace(text, @"\*\*(.+?)\*\*", "$1");
            regex = Regex.Replace(regex, @"__(.+?)__", "$1");
            regex = Regex.Replace(regex, @"_(.+?)_", "$1");
            regex = Regex.Replace(regex, @"`(.+?)`", "$1");
            regex = Regex.Replace(regex, @"\[(.+?)\]\(.+?\)", "$1");
            
            return regex;
        }

        private static void DrawImage(IDalamudTextureWrap? wrap, string text) {
            if (wrap != null) {
                var avail = ImGui.GetContentRegionAvail().X;
                var width  = Math.Min(wrap.Width, avail);
                var height  = wrap.Height * (width / wrap.Width);
                
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (avail - width) * 0.5f);
                ImGui.Image(wrap.Handle, new Vector2(width, height));

                if (ImGui.IsItemHovered()) {
                    using var tooltip = ImRaii.Tooltip(); ImGui.TextUnformatted(text);
                }
            } else {
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.5f, 0.5f, 0.5f, 1f));
                CenteredText($"[ {text} — loading... ]");
                ImGui.PopStyleColor();
            }
            
            ImGui.Spacing();
            ImGui.Spacing();
        }
    }
}
