using System;
using System.IO;
using System.Net.Http;
using System.Numerics;
using System.Threading.Tasks;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace Skippy.UI {
    internal enum Category {
        MSQRoulette,
        MassivePC,
        GoldSaucer,
        CustomTalk,
        NormalCutscenes,
        FeedBuddy,
        RiskySkips,
        AutoParty,
        Logger,
        Commands,
        About
    }

    internal sealed partial class Menu : Window, IDisposable {
        private readonly Config _config;
        private readonly Action<bool> _setEnabled;
        private readonly Action _saveConfig;
        private readonly IDalamudPluginInterface _pluginInterface;
        private readonly ITextureProvider _tex;

        private Category _category = Category.MSQRoulette;
        private ISharedImmediateTexture? _icon;

        internal static readonly HttpClient Http = new();

        internal Menu(Config config, Action<bool> setEnabled, Action saveConfig, IDalamudPluginInterface pluginInterface, ITextureProvider tex) : base("Skippy  |  v2.2.1.0###SkippyMain", ImGuiWindowFlags.NoScrollbar, forceMainWindow: false) {
            _config = config;
            _setEnabled = setEnabled;
            _saveConfig = saveConfig;
            _pluginInterface = pluginInterface;
            _tex = tex;

            SizeConstraints = new WindowSizeConstraints {
                MinimumSize = new Vector2(550, 450),
                MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
            };

            AllowPinning = true;

            TitleBarButtons.Add(new TitleBarButton {
                Icon = FontAwesomeIcon.Heart,
                IconOffset = new Vector2(0, 1),
                ShowTooltip = () => {
                    using var tooltip = Dalamud.Interface.Utility.Raii.ImRaii.Tooltip();
                    ImGui.TextUnformatted("Support Skippy on Ko-fi");
                },
                Click = _ => OpenUrl("https://ko-fi.com/boxu_chan")
            });
        }

        public void Dispose() { }

        private bool _theme;
        private const int ThemeColor = 10;

        public override void PreDraw() {
            if (!_theme) {
                ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0.06f, 0.06f, 0.10f, 0.97f));
                ImGui.PushStyleColor(ImGuiCol.ChildBg, new Vector4(0.04f, 0.04f, 0.08f, 1.00f));
                ImGui.PushStyleColor(ImGuiCol.Border, new Vector4(0.30f, 0.30f, 0.55f, 0.60f));
                ImGui.PushStyleColor(ImGuiCol.Header, new Vector4(0.20f, 0.20f, 0.45f, 0.55f));
                ImGui.PushStyleColor(ImGuiCol.HeaderHovered, new Vector4(0.28f, 0.28f, 0.65f, 0.80f));
                ImGui.PushStyleColor(ImGuiCol.HeaderActive, new Vector4(0.22f, 0.22f, 0.60f, 1.00f));
                ImGui.PushStyleColor(ImGuiCol.FrameBg, new Vector4(0.14f, 0.14f, 0.28f, 0.54f));
                ImGui.PushStyleColor(ImGuiCol.FrameBgHovered, new Vector4(0.22f, 0.22f, 0.50f, 0.40f));
                ImGui.PushStyleColor(ImGuiCol.CheckMark, new Vector4(0.55f, 0.55f, 1.00f, 1.00f));
                ImGui.PushStyleColor(ImGuiCol.TitleBgActive, new Vector4(0.10f, 0.10f, 0.30f, 1.00f));
                
                _theme = true;
            }
        }

        public override void PostDraw() {
            if (_theme) {
                ImGui.PopStyleColor(ThemeColor); 
                _theme = false;
            }
        }

        public override void Draw() {
            var style = ImGui.GetStyle();
            var footerHeight = ImGui.GetTextLineHeight() + style.ItemSpacing.Y * 2f;
            var topHeight = ImGui.GetContentRegionAvail().Y - footerHeight;

            if (!ImGui.BeginTable("##Layout", 2, ImGuiTableFlags.None)) {
                return;
            }

            try {
                ImGui.TableSetupColumn("##Menu", ImGuiTableColumnFlags.WidthFixed, 200f);
                ImGui.TableSetupColumn("##Content", ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableNextColumn();

                var beforePowerY = ImGui.GetCursorPosY();

                DrawPowerButton();

                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();

                var topUsed = ImGui.GetCursorPosY() - beforePowerY;

                ImGui.PushStyleVar(ImGuiStyleVar.ScrollbarSize, 14f);
                ImGui.PushStyleVar(ImGuiStyleVar.ScrollbarRounding, 6f);
                ImGui.PushStyleColor(ImGuiCol.ScrollbarBg, new Vector4(0.06f, 0.06f, 0.12f, 0.90f));
                ImGui.PushStyleColor(ImGuiCol.ScrollbarGrab, new Vector4(0.32f, 0.32f, 0.70f, 0.95f));
                ImGui.PushStyleColor(ImGuiCol.ScrollbarGrabHovered, new Vector4(0.45f, 0.45f, 0.90f, 1.00f));
                ImGui.PushStyleColor(ImGuiCol.ScrollbarGrabActive, new Vector4(0.60f, 0.60f, 1.00f, 1.00f));

                var menuFlags = ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse;

                if (ImGui.BeginChild("##Categories", new Vector2(0, topHeight - topUsed), false, menuFlags)) {
                    bool error = !Skippy.Instance.Address.Valid;
                    NavItem("MSQ Roulette", Category.MSQRoulette, _config.SkipMSQRoulette || _config.SkipOceanFishing || _config.SkipCrystallineConflict || _config.ExemptPrae || _config.ExemptCastrum || _config.ExemptPorta, error);
                    NavItem("Large-Scale Content", Category.MassivePC, _config.SkipMassivePC, error);
                    NavItem("Gold Saucer", Category.GoldSaucer, _config.SkipGoldSaucer || _config.ExemptChocoboRace || _config.ExemptVerminion || _config.ExemptTripleTriad || _config.ExemptFallGuys, error);
                    NavItem("NPC Dialogue", Category.CustomTalk, _config.SkipCustomTalk || _config.ExemptSubmarines, error);
                    NavItem("World Cutscenes", Category.NormalCutscenes, _config.SkipNormalCutscenes, error);
                    NavItem("Feed Buddy", Category.FeedBuddy, _config.SkipFeedBuddy, error);

                    ImGui.Spacing(); 
                    ImGui.Separator(); 
                    ImGui.Spacing();

                    var risky = !_config.IsEnabled;
                    
                    if (risky) {
                        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.5f, 0.5f, 0.5f, 0.6f));
                        ImGui.PushStyleColor(ImGuiCol.HeaderHovered, new Vector4(0.15f, 0.15f, 0.15f, 0.4f));
                        ImGui.PushStyleColor(ImGuiCol.HeaderActive, new Vector4(0.10f, 0.10f, 0.10f, 0.4f));
                    } else {
                        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1.0f, 0.45f, 0.35f, 1.0f));
                        ImGui.PushStyleColor(ImGuiCol.HeaderHovered, new Vector4(0.35f, 0.10f, 0.05f, 0.8f));
                        ImGui.PushStyleColor(ImGuiCol.HeaderActive, new Vector4(0.30f, 0.08f, 0.04f, 1.0f));
                    }

                    var riskyHeight = ImGui.GetTextLineHeight() + ImGui.GetStyle().ItemSpacing.Y * 3f;
                    var riskyPos = ImGui.GetCursorScreenPos();
                    var riskyWidth = ImGui.GetContentRegionAvail().X;
                    var padding = ImGui.GetStyle().FramePadding.X;

                    if (_category == Category.RiskySkips) {
                        var drawList = ImGui.GetWindowDrawList();
                        drawList.AddRectFilled(riskyPos, new Vector2(riskyPos.X + riskyWidth, riskyPos.Y + riskyHeight), ImGui.GetColorU32(ImGuiCol.Header));
                    }

                    ImGui.SetCursorScreenPos(riskyPos);
                    ImGui.InvisibleButton("##RiskySkipsButtons", new Vector2(riskyWidth, riskyHeight));

                    if (ImGui.IsItemHovered()) {
                        var drawList = ImGui.GetWindowDrawList();
                        drawList.AddRectFilled(riskyPos, new Vector2(riskyPos.X + riskyWidth, riskyPos.Y + riskyHeight), ImGui.GetColorU32(ImGuiCol.HeaderHovered));
                    }

                    if (ImGui.IsItemClicked()) {
                        if (!_config.HideWarning) _showPopup = true;
                        else _category = Category.RiskySkips;
                    }

                    {
                        var drawList = ImGui.GetWindowDrawList();
                        var icon = FontAwesomeIcon.ExclamationTriangle.ToIconString();
                        float width;

                        using (_pluginInterface.UiBuilder.IconFontHandle.Push()) {
                            width = ImGui.CalcTextSize(icon).X;
                        }

                        var iconColor = risky ? ImGui.ColorConvertFloat4ToU32(new Vector4(0.5f, 0.5f, 0.5f, 0.6f)) : ImGui.ColorConvertFloat4ToU32(new Vector4(1.0f, 0.45f, 0.35f, 1.0f));
                        var textColor = iconColor;

                        var x = riskyPos.X + padding + 8f;
                        var y = riskyPos.Y + (riskyHeight - ImGui.GetTextLineHeight()) * 0.5f;
                        var textPosition = x + width + 6f;

                        drawList.AddText(_pluginInterface.UiBuilder.IconFontHandle.Lock().ImFont, ImGui.GetFontSize(), new Vector2(x, y), iconColor, icon);
                        drawList.AddText(ImGui.GetFont(), ImGui.GetFontSize(), new Vector2(textPosition, y), textColor, "Risky Skips");
                    }

                    ImGui.PopStyleColor(3);
                    
                    ImGui.Spacing(); 
                    ImGui.Separator(); 
                    ImGui.Spacing();

                    IconNavItem("Auto-Party Mode", Category.AutoParty, FontAwesomeIcon.Users, new Vector4(0.70f, 0.70f, 0.70f, 1.0f));
                    IconNavItem("Logger", Category.Logger, FontAwesomeIcon.FileAlt, new Vector4(0.70f, 0.70f, 0.70f, 1.0f));

                    ImGui.Spacing(); 
                    ImGui.Separator(); 
                    ImGui.Spacing();

                    IconNavItem("Commands", Category.Commands, FontAwesomeIcon.Terminal, new Vector4(0.70f, 0.70f, 0.70f, 1.0f));
                    IconNavItem("About", Category.About, FontAwesomeIcon.InfoCircle, new Vector4(0.70f, 0.70f, 0.70f, 1.0f));
                }
                
                ImGui.EndChild();

                ImGui.PopStyleColor(4);
                ImGui.PopStyleVar(2);

                ImGui.TableNextColumn();

                var flags = ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.AlwaysVerticalScrollbar;

                ImGui.PushStyleVar(ImGuiStyleVar.ScrollbarSize, 14f);
                ImGui.PushStyleVar(ImGuiStyleVar.ScrollbarRounding, 6f);
                ImGui.PushStyleColor(ImGuiCol.ScrollbarBg, new Vector4(0.06f, 0.06f, 0.12f, 0.90f));
                ImGui.PushStyleColor(ImGuiCol.ScrollbarGrab, new Vector4(0.32f, 0.32f, 0.70f, 0.95f));
                ImGui.PushStyleColor(ImGuiCol.ScrollbarGrabHovered, new Vector4(0.45f, 0.45f, 0.90f, 1.00f));
                ImGui.PushStyleColor(ImGuiCol.ScrollbarGrabActive, new Vector4(0.60f, 0.60f, 1.00f, 1.00f));

                if (ImGui.BeginChild("##Skips", new Vector2(0, topHeight), false, flags)) {
                    ImGui.SetCursorPos(ImGui.GetCursorPos() + new Vector2(10f, 8f));
                    ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X - 20f);
                    ImGui.BeginGroup();

                    bool disabled = !_config.IsEnabled && !(_config.AutoEnable4Man && _config.IsEnabled && _category == Category.MSQRoulette);
                    
                    if (disabled) {
                        ImGui.BeginDisabled();
                    }

                    switch (_category) {
                        case Category.MSQRoulette:
                            UIMSQRoulette();
                            break;
                        
                        case Category.MassivePC:
                            UIMassivePC();
                            break;
                        
                        case Category.GoldSaucer:
                            UIGoldSaucer();
                            break;
                        
                        case Category.CustomTalk:
                            UICustomTalk();
                            break;
                        
                        case Category.NormalCutscenes:
                            UINormalCutscenes();
                            break;
                        
                        case Category.FeedBuddy:
                            UIFeedBuddy();
                            break;
                        
                        case Category.RiskySkips:
                            UIRiskySkips();
                            break;
                        
                        case Category.AutoParty:
                            UIAutoParty();
                            break;
                        
                        case Category.Logger:
                            UILogger();
                            break;
                        
                        case Category.Commands:
                            UICommands();
                            break;
                        
                        case Category.About:
                            UIAbout();
                            break;
                    }

                    if (disabled) {
                        ImGui.EndDisabled();
                    }

                    ImGui.Spacing();
                    ImGui.Spacing();
                    ImGui.EndGroup();
                    ImGui.PopItemWidth();
                }

                ImGui.EndChild();

                ImGui.PopStyleColor(4);
                ImGui.PopStyleVar(2);
            } catch (Exception e) {
                ImGui.TextColored(new Vector4(1, 0.3f, 0.3f, 1), $"UI Error: {e.Message}");
            }

            ImGui.EndTable();

            var footerText  = "© Boxu - 2026 | Dalamud 15.0.0 (Patch 7.5)";
            var footerAvail = ImGui.GetContentRegionAvail().X;
            
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + footerAvail - ImGui.CalcTextSize(footerText).X - 4f);
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.4f, 0.4f, 0.6f, 0.7f));
            ImGui.TextUnformatted(footerText);
            ImGui.PopStyleColor();

            DrawPopup();
        }

        private bool _showPopup;
        private bool _popupOpen;

        private void DrawPopup() {
            if (_showPopup) {
                ImGui.OpenPopup("##Popup");
                _showPopup = false;
                _popupOpen = true;
            }

            ImGui.SetNextWindowSize(new Vector2(420, 0), ImGuiCond.Always);
            
            if (ImGui.BeginPopupModal("##Popup", ref _popupOpen, ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize)) {
                ImGui.Spacing();
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1.0f, 0.40f, 0.30f, 1f));

                var icon = FontAwesomeIcon.ExclamationTriangle.ToIconString();
                float iconWidth;
                
                using (_pluginInterface.UiBuilder.IconFontHandle.Push()) {
                    iconWidth = ImGui.CalcTextSize(icon).X;
                }

                var labelWidth = ImGui.CalcTextSize(" Warning").X;
                var width = iconWidth + labelWidth;
                var avail = ImGui.GetContentRegionAvail().X;
                
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (avail - width) * 0.5f);

                using (_pluginInterface.UiBuilder.IconFontHandle.Push()) {
                    ImGui.TextUnformatted(icon);
                }

                ImGui.SameLine(0, 4f);
                ImGui.TextUnformatted(" Warning");
                ImGui.PopStyleColor();
                ImGui.Spacing();
                
                ImGui.TextWrapped("The skips contained within this category are very experimental and may carry a heavy ban risk.\n\n" + "By enabling any of these, you accept full responsibility for any consequences, I, the developer, have no responsibility in your choice of usages.");
                
                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();

                bool hide = _config.HideWarning;
                
                if (ImGui.Checkbox("Don't show this again!", ref hide)) {
                    _config.HideWarning = hide;
                    _saveConfig();
                }
                
                ImGui.SameLine();
                ImGui.SetCursorPosX(ImGui.GetWindowWidth() - 150f - ImGui.GetStyle().WindowPadding.X);
                
                if (ImGui.Button("I Agree & Understand", new Vector2(150f, 0))) {
                    _category = Category.RiskySkips;
                    ImGui.CloseCurrentPopup();
                }

                ImGui.EndPopup();
            }
        }

        private void DrawPowerButton() {
            var path = Path.Combine(_pluginInterface.AssemblyLocation.DirectoryName!, "icon.png");
            
            if (_icon == null && File.Exists(path)) {
                try {
                    _icon = _tex.GetFromFile(path);
                } catch { }
            }

            IDalamudTextureWrap? icon = null;
            _icon?.TryGetWrap(out icon, out _);

            const float button = 120f;
            var width = ImGui.GetContentRegionAvail().X;

            ImGui.Spacing();
            ImGui.Spacing();
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (width - button) * 0.5f);

            bool hasError = !Skippy.Instance.Address.Valid;
            var tint = hasError ? new Vector4(1.0f, 0.55f, 0.10f, 1.00f) : _config.IsEnabled ? new Vector4(1f, 1f, 1f, 1f) : new Vector4(0.30f, 0.30f, 0.30f, 0.80f);
            var border = hasError ? new Vector4(0.80f, 0.40f, 0.05f, 0.90f) : _config.IsEnabled ? new Vector4(0.40f, 0.40f, 0.90f, 0.80f) : new Vector4(0.22f, 0.22f, 0.22f, 0.60f);

            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0f, 0f, 0f, 0f));
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.15f, 0.15f, 0.30f, 0.80f));
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.10f, 0.10f, 0.25f, 1.00f));
            ImGui.PushStyleColor(ImGuiCol.Border, border);
            ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 2f);
            ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 8f);

            bool clicked = icon != null ? ImGui.ImageButton(icon.Handle, new Vector2(button, button), Vector2.Zero, Vector2.One, 2, new Vector4(0f, 0f, 0f, 0f), tint) : ImGui.Button(_config.IsEnabled ? "ON" : "OFF", new Vector2(button, button));

            ImGui.PopStyleVar(2);
            ImGui.PopStyleColor(4);

            if (clicked && !hasError) {
                _config.IsEnabled = !_config.IsEnabled;
                _setEnabled(_config.IsEnabled);
                _saveConfig();
            }

            if (ImGui.IsItemHovered()) {
                using var tooltip = ImRaii.Tooltip();
                ImGui.TextUnformatted(hasError ? "[ERROR] — Cutscene offsets not found, the plugin will not work" : _config.IsEnabled ? "[ENABLED] — Click to Disable Skippy" : "[DISABLED] — Click to Enable Skippy");
            }

        }

        private void NavItem(string label, Category target, bool active, bool hasError = false) {
            var height = ImGui.GetTextLineHeight() + ImGui.GetStyle().ItemSpacing.Y * 3f;
            bool greyOut = !_config.IsEnabled && !(_config.AutoEnable4Man && _config.IsEnabled && target == Category.MSQRoulette);

            var iconColor = greyOut ? new Vector4(0.5f, 0.5f, 0.5f, 0.4f) : !active ? new Vector4(0.70f, 0.20f, 0.20f, 1.0f) : hasError ? new Vector4(1.0f, 0.55f, 0.10f, 1.0f) : new Vector4(0.20f, 0.80f, 0.20f, 1.0f);
            var icon = hasError && active ? FontAwesomeIcon.ExclamationCircle.ToIconString() : FontAwesomeIcon.Circle.ToIconString();

            if (greyOut) {
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.5f, 0.5f, 0.5f, 0.6f));
                ImGui.PushStyleColor(ImGuiCol.HeaderHovered, new Vector4(0.15f, 0.15f, 0.15f, 0.4f));
                ImGui.PushStyleColor(ImGuiCol.HeaderActive, new Vector4(0.10f, 0.10f, 0.10f, 0.4f));
            }

            var cursor = ImGui.GetCursorScreenPos();
            var drawList = ImGui.GetWindowDrawList();
            var padding = ImGui.GetStyle().FramePadding.X;

            float width;

            using (_pluginInterface.UiBuilder.IconFontHandle.Push()) {
                width = ImGui.CalcTextSize(icon).X;
            }

            ImGui.PushStyleVar(ImGuiStyleVar.SelectableTextAlign, new Vector2(0f, 0.5f));
            
            if (ImGui.Selectable($"##{target}", _category == target, ImGuiSelectableFlags.None, new Vector2(0, height))) {
                _category = target;
            }
            
            ImGui.PopStyleVar();

            var leftPadding = 8f;
            var iconX = cursor.X + padding + leftPadding;
            var iconY = cursor.Y + (height - ImGui.GetTextLineHeight()) * 0.5f;
            var textX = iconX + width + 8f;
            var textY = iconY;

            drawList.AddText(_pluginInterface.UiBuilder.IconFontHandle.Lock().ImFont, ImGui.GetFontSize(), new Vector2(iconX, iconY), ImGui.ColorConvertFloat4ToU32(iconColor), icon);
            drawList.AddText(ImGui.GetFont(), ImGui.GetFontSize(), new Vector2(textX, textY), ImGui.GetColorU32(ImGuiCol.Text), label);

            if (greyOut) {
                ImGui.PopStyleColor(3);
            }
        }

        private void IconNavItem(string label, Category target, FontAwesomeIcon faIcon, Vector4 iconColor) {
            var height = ImGui.GetTextLineHeight() + ImGui.GetStyle().ItemSpacing.Y * 3f;
            bool greyOut = !_config.IsEnabled;

            if (greyOut) {
                iconColor = new Vector4(0.5f, 0.5f, 0.5f, 0.4f);
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.5f, 0.5f, 0.5f, 0.6f));
                ImGui.PushStyleColor(ImGuiCol.HeaderHovered, new Vector4(0.15f, 0.15f, 0.15f, 0.4f));
                ImGui.PushStyleColor(ImGuiCol.HeaderActive, new Vector4(0.10f, 0.10f, 0.10f, 0.4f));
            }

            var cursor = ImGui.GetCursorScreenPos();
            var drawList = ImGui.GetWindowDrawList();
            var padding = ImGui.GetStyle().FramePadding.X;
            var icon = faIcon.ToIconString();

            float width;

            using (_pluginInterface.UiBuilder.IconFontHandle.Push()) {
                width = ImGui.CalcTextSize(icon).X;
            }

            ImGui.PushStyleVar(ImGuiStyleVar.SelectableTextAlign, new Vector2(0f, 0.5f));

            if (ImGui.Selectable($"##{target}", _category == target, ImGuiSelectableFlags.None, new Vector2(0, height))) {
                _category = target;
            }
            
            ImGui.PopStyleVar();

            var leftPadding = 8f;
            var iconX = cursor.X + padding + leftPadding;
            var iconY = cursor.Y + (height - ImGui.GetTextLineHeight()) * 0.5f;
            var textX = iconX + width + 8f;
            var textY = iconY;

            drawList.AddText(_pluginInterface.UiBuilder.IconFontHandle.Lock().ImFont, ImGui.GetFontSize(), new Vector2(iconX, iconY), ImGui.ColorConvertFloat4ToU32(iconColor), icon);
            drawList.AddText(ImGui.GetFont(), ImGui.GetFontSize(), new Vector2(textX, textY), ImGui.GetColorU32(ImGuiCol.Text), label);

            if (greyOut) {
                ImGui.PopStyleColor(3);
            }
        }

        internal static void OpenUrl(string url) {
            try {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url) {
                    UseShellExecute = true
                });
            } catch { }
        }

        internal void FeatureCheckbox(string id, string label, string description, ref bool value, out bool changed) {
            changed = false;
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(8, ImGui.GetStyle().ItemSpacing.Y));

            if (ImGui.Checkbox(id, ref value)) {
                changed = true;
            }
            
            ImGui.PopStyleVar();
            ImGui.SameLine();
            ImGui.TextUnformatted(label);
            ImGui.SameLine();
            
            ImGuiComponents.HelpMarker(description);
            
            ImGui.Spacing(); 
            ImGui.Spacing();
        }

        internal static void SectionHeader(string text) {
            var width = ImGui.GetContentRegionAvail().X;
            var textWidth = ImGui.CalcTextSize(text).X;
            
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (width - textWidth) * 0.5f);
            ImGui.TextColored(new Vector4(0.75f, 0.75f, 1.00f, 1f), text);
            
            ImGui.Separator();
            ImGui.Spacing(); 
            ImGui.Spacing();
        }

        internal static void CenteredText(string text) {
            var width = ImGui.GetContentRegionAvail().X;
            var textWidth = ImGui.CalcTextSize(text).X;
            
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (width - textWidth) * 0.5f);
            ImGui.Text(text);
        }

        internal static void WipToggle(string label) {
            ImGui.BeginDisabled();
            bool dummy = false;
            
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(8, ImGui.GetStyle().ItemSpacing.Y));
            ImGui.Checkbox($"##{label}", ref dummy);
            ImGui.PopStyleVar();
            ImGui.SameLine();
            ImGui.TextUnformatted($"{label}  [Work in Progress]");
            ImGui.EndDisabled();
            
            if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled)) {
                using var tooltip = ImRaii.Tooltip();
                ImGui.TextUnformatted("This feature is not yet available.");
            }
            
            ImGui.Spacing(); 
            ImGui.Spacing();
        }

        internal async Task LoadRemoteImage(string url, Action<IDalamudTextureWrap> onLoaded) {
            try {
                var bytes = await Http.GetByteArrayAsync(url).ConfigureAwait(false);
                var wrap = await _tex.CreateFromImageAsync(bytes).ConfigureAwait(false);
                onLoaded(wrap);
            } catch { }
        }
    }
}
