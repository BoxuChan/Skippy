using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Numerics;
using System.Text;
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
using Skippy.Skips;

namespace Skippy.UI {
    internal enum Category {
        MSQRoulette,
        MassivePC,
        GoldSaucer,
        CustomTalk,
        NormalCutscenes,
        FeedBuddy,
        RiskySkips,
        HookResearch,
        AutoParty,
        Behaviour,
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

        internal Menu(Config config, Action<bool> setEnabled, Action saveConfig, IDalamudPluginInterface pluginInterface, ITextureProvider tex) : base("Skippy  |  v2.2.4.2###SkippyMain", ImGuiWindowFlags.NoScrollbar, forceMainWindow: false) {
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
                    using var tooltip = ImRaii.Tooltip();
                    ImGui.TextUnformatted("Support Skippy on Ko-fi");
                },
                
                Click = _ => OpenUrl("https://ko-fi.com/boxu_chan")
            });
            
            TitleBarButtons.Add(new TitleBarButton {
                Icon = FontAwesomeIcon.Cog,
                IconOffset = new Vector2(0, 1),
                
                ShowTooltip = () => {
                    using var tooltip = ImRaii.Tooltip();
                    ImGui.TextUnformatted(_config.DevMode ? "Developer Mode: ON" : "Developer Mode: OFF");
                },
                
                Click = _ => {
                    if (_config.DevMode) {
                        _config.DevMode = false;
                        _config.DevPassword = string.Empty;
                        _config.ResearchMSQHook = false;
                        _config.ResearchMassivePCHook = false;
                        _config.ResearchGoldSaucerHook = false;
                        _config.ResearchCustomTalkHook = false;
                        _config.ResearchNormalCutscenesHook = false;
                        _config.ResearchInnHook = false;
                        _config.ResearchFeedBuddyHook = false;
                        _config.ResearchGrandCompanyRankUpHook = false;
                        _config.ResearchHairMakeHook = false;
                        _config.ResearchExtraLogs = false;
                        
                        if (_category == Category.HookResearch) {
                            _category = Category.MSQRoulette;
                        }
                        
                        Skippy.Instance.Hooks.RefreshHooks();
                        _saveConfig();
                        
                        Skippy.Instance.PrintError("[Skippy] Developer Mode has been disabled along with any tweaks you may have had enabled there.");
                    } else {
                        _devPasswordInput = string.Empty;
                        _devPasswordPending = false;
                        _showDevPopup = true;
                    }
                }
            });
            
            TitleBarButtons.Add(new TitleBarButton { 
                Icon = FontAwesomeIcon.Star, 
                IconOffset = new Vector2(0, 1),
                ShowTooltip = () => {
                    using var tooltip = ImRaii.Tooltip();
                    ImGui.TextUnformatted(_config.HideBadge ? "Show Badge" : "Hide Badge");
                },
                    
                Click = _ => {
                    _config.HideBadge = !_config.HideBadge;
                    _saveConfig();
                }
            });
        }

        internal void ResetCategory() => _category = Category.MSQRoulette;

        public void Dispose() { }

        private bool _theme;
        private const int ThemeColor = 9;

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

            Vector2 _skipsChildScreenPos = default;
            Vector2 _skipsChildScreenSize = default;

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
                ImGui.PushStyleColor(ImGuiCol.ScrollbarGrab, new Vector4(0.20f, 0.20f, 0.50f, 0.90f));
                ImGui.PushStyleColor(ImGuiCol.ScrollbarGrabHovered, new Vector4(0.24f, 0.24f, 0.55f, 0.95f));
                ImGui.PushStyleColor(ImGuiCol.ScrollbarGrabActive, new Vector4(0.24f, 0.24f, 0.55f, 0.95f));

                if (ImGui.BeginChild("##Categories", new Vector2(0, topHeight - topUsed), false, ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize)) {
                    bool error = !Skippy.Instance.Address.Valid;
                    
                    NavItem("MSQ Roulette", Category.MSQRoulette, _config.SkipMSQRoulette || _config.ExemptPrae || _config.ExemptCastrum || _config.ExemptPorta, error, _config.AutoEnable4Man ? new Vector4(0.90f, 0.85f, 0.55f, 1f) : null);
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

                    var riskyHeight = ImGui.GetTextLineHeight() + ImGui.GetStyle().ItemSpacing.Y * 4f;
                    var riskyPos = ImGui.GetCursorScreenPos();
                    var riskyWidth = ImGui.GetContentRegionAvail().X;
                    var padding = ImGui.GetStyle().FramePadding.X;

                    if (_category == Category.RiskySkips) {
                        var drawList = ImGui.GetWindowDrawList();
                        drawList.AddRectFilled(riskyPos, new Vector2(riskyPos.X + riskyWidth, riskyPos.Y + riskyHeight), ImGui.GetColorU32(ImGuiCol.HeaderHovered));
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
                    
                    if (_config.DevMode) {
                        IconNavItem("Hook Research", Category.HookResearch, FontAwesomeIcon.Flask, new Vector4(0.40f, 0.60f, 1.0f, 1.0f), true);
                    }

                    ImGui.Spacing(); 
                    ImGui.Separator(); 
                    ImGui.Spacing();

                    IconNavItem("Auto-Party Mode", Category.AutoParty, FontAwesomeIcon.Users, new Vector4(0.70f, 0.70f, 0.70f, 1.0f));
                    IconNavItem("Behaviour", Category.Behaviour, FontAwesomeIcon.SlidersH, new Vector4(0.70f, 0.70f, 0.70f, 1.0f));
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

                ImGui.PushStyleVar(ImGuiStyleVar.ScrollbarSize, 14f);
                ImGui.PushStyleVar(ImGuiStyleVar.ScrollbarRounding, 6f);
                ImGui.PushStyleColor(ImGuiCol.ScrollbarBg, new Vector4(0.06f, 0.06f, 0.12f, 0.90f));
                ImGui.PushStyleColor(ImGuiCol.ScrollbarGrab, new Vector4(0.20f, 0.20f, 0.50f, 0.90f));
                ImGui.PushStyleColor(ImGuiCol.ScrollbarGrabHovered, new Vector4(0.24f, 0.24f, 0.55f, 0.95f));
                ImGui.PushStyleColor(ImGuiCol.ScrollbarGrabActive, new Vector4(0.24f, 0.24f, 0.55f, 0.95f));

                var flags = ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.AlwaysVerticalScrollbar;
                
                if (ImGui.BeginChild("##Skips", new Vector2(0, topHeight), false, flags)) {
                    _skipsChildScreenPos = ImGui.GetWindowPos();
                    _skipsChildScreenSize = ImGui.GetWindowSize();
                    
                    ImGui.SetCursorPos(ImGui.GetCursorPos() + new Vector2(10f, 8f));
                    ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X - 20f);
                    ImGui.BeginGroup();
                    ImGui.PushTextWrapPos(ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X - 10f);

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
                        
                        case Category.HookResearch:
                            UIHookResearch();
                            break;
                        
                        case Category.AutoParty:
                            UIAutoParty();
                            break;
                        
                        case Category.Behaviour:
                            UIBehaviour();
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
                    
                    var _testerInfo = Skippy.Instance.TesterInfo;
                    
                    if (_skipsChildScreenSize != default && !_config.HideBadge) {
                        DrawTesterBadgeButton(_skipsChildScreenPos, _skipsChildScreenSize, _testerInfo);
                    }
                    
                    ImGui.Spacing(); 
                    ImGui.Spacing();
                    ImGui.EndGroup();
                    ImGui.PopTextWrapPos();
                    ImGui.PopItemWidth();
                }
                
                if (_skipsChildScreenSize != default && !_config.HideBadge) {
                    DrawTesterBadgeImage(_skipsChildScreenPos, _skipsChildScreenSize, Skippy.Instance.TesterInfo);
                }
                
                ImGui.EndChild();
                
                ImGui.PopStyleColor(4);
                ImGui.PopStyleVar(2);
            } catch (Exception e) {
                ImGui.TextColored(new Vector4(1, 0.3f, 0.3f, 1), $"UI Error: {e.Message}");
            }

            ImGui.EndTable();

            var footerText = "© Boxu - 2026 | Dalamud 15.0.0 (Patch 7.5)";
            var footerAvail = ImGui.GetContentRegionAvail().X;
            var footerY = ImGui.GetCursorPosY();

            ImGui.SetCursorPos(new Vector2(ImGui.GetCursorPosX() + footerAvail - ImGui.CalcTextSize(footerText).X - 4f, footerY));
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.4f, 0.4f, 0.6f, 0.7f));
            ImGui.TextUnformatted(footerText);
            ImGui.PopStyleColor();

            DrawPopup();
            DrawDevPopup();
            DrawTesterPopup();
        }

        private bool _showDevPopup;
        private bool _devPopupOpen;
        private string _devPasswordInput = string.Empty;
        private bool _devPasswordPending;
        private bool _showPopup;
        private bool _popupOpen;
        private bool _showTesterPopup;
        private bool _testerPopupOpen;
        private ISharedImmediateTexture? _badgeIcon;
        private ISharedImmediateTexture? _badgeGreyIcon;

        private const float BadgeSize = 36f;
        private const float BadgePadRight = 20f;
        private const float BadgePadBot = 8f;

        private static Vector2 CalcBadgeScreenPos(Vector2 childScreenPos, Vector2 childScreenSize) => new Vector2(childScreenPos.X + childScreenSize.X - BadgeSize - BadgePadRight, childScreenPos.Y + childScreenSize.Y - BadgeSize - BadgePadBot);

        private void DrawTesterBadgeButton(Vector2 childScreenPos, Vector2 childScreenSize, TesterInfo? testerInfo) {
            var screenPos = CalcBadgeScreenPos(childScreenPos, childScreenSize);

            ImGui.SetCursorScreenPos(screenPos);
            bool clicked = ImGui.InvisibleButton("##TesterBadgeOverlay", new Vector2(BadgeSize, BadgeSize));
            bool hovered = ImGui.IsItemHovered();

            if (testerInfo != null) {
                if (clicked) {
                    _showTesterPopup = true;
                }

                if (hovered) {
                    using var tooltip = ImRaii.Tooltip();
                    ImGui.TextUnformatted($"Skippy Tester  |  {testerInfo.Nickname}");
                }
            } else {
                if (hovered) {
                    using var tooltip = ImRaii.Tooltip();
                    ImGui.TextUnformatted("You aren't a Skippy Tester!");
                }
            }
        }

        private void DrawTesterBadgeImage(Vector2 childScreenPos, Vector2 childScreenSize, TesterInfo? testerInfo) {
            var dir = _pluginInterface.AssemblyLocation.DirectoryName!;
            bool isTester = testerInfo != null;

            if (isTester) {
                if (_badgeIcon == null) {
                    var path = Path.Combine(dir, "badge.png");
                    
                    if (File.Exists(path)) {
                        try {
                            _badgeIcon = _tex.GetFromFile(path);
                        } catch { }
                    }
                }
            } else {
                if (_badgeGreyIcon == null) {
                    var path = Path.Combine(dir, "badge_grey.png");
                    
                    if (File.Exists(path)) {
                        try {
                            _badgeGreyIcon = _tex.GetFromFile(path);
                        } catch { }
                    }
                }
            }

            var screenPos = CalcBadgeScreenPos(childScreenPos, childScreenSize);
            IDalamudTextureWrap? badgeWrap = null;
            
            if (isTester) {
                _badgeIcon?.TryGetWrap(out badgeWrap, out _);
            } else {
                _badgeGreyIcon?.TryGetWrap(out badgeWrap, out _);
            }

            var drawList = ImGui.GetWindowDrawList();
            drawList.PushClipRect(childScreenPos, childScreenPos + childScreenSize, false);

            var mouse = ImGui.GetIO().MousePos;
            bool hovered = mouse.X >= screenPos.X && mouse.X <= screenPos.X + BadgeSize && mouse.Y >= screenPos.Y && mouse.Y <= screenPos.Y + BadgeSize;

            if (badgeWrap != null) {
                var badgeEnd = screenPos + new Vector2(BadgeSize, BadgeSize);
                drawList.AddImage(badgeWrap.Handle, screenPos, badgeEnd, Vector2.Zero, Vector2.One, 0xFFFFFFFF);

                if (isTester && hovered) {
                    drawList.AddImage(badgeWrap.Handle, screenPos, badgeEnd, Vector2.Zero, Vector2.One, 0x66FFFFFF);
                }
            } else {
                uint starCol = isTester ? (hovered ? 0xFF44DDFF : 0xFF22BBEE) : 0xFF888888;
                drawList.AddText(ImGui.GetFont(), BadgeSize, screenPos, starCol, "★");
            }

            drawList.PopClipRect();
        }

        private void DrawTesterPopup() {
            var testerInfo = Skippy.Instance.TesterInfo;
            
            if (testerInfo == null) {
                return;
            }

            if (_showTesterPopup) {
                ImGui.OpenPopup("##TesterPopup");
                _showTesterPopup = false;
                _testerPopupOpen = true;
            }

            ImGui.SetNextWindowSize(new Vector2(400, 0), ImGuiCond.Always);

            if (ImGui.BeginPopupModal("##TesterPopup", ref _testerPopupOpen, ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize)) {
                ImGui.Spacing();

                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.90f, 0.80f, 0.30f, 1f));

                var starIcon = FontAwesomeIcon.Star.ToIconString();
                float starWidth;
                
                using (_pluginInterface.UiBuilder.IconFontHandle.Push()) {
                    starWidth = ImGui.CalcTextSize(starIcon).X;
                }

                var titleLabel = " Special Thanks!";
                var titleWidth = starWidth + ImGui.CalcTextSize(titleLabel).X + 4f;
                var avail = ImGui.GetContentRegionAvail().X;
                
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (avail - titleWidth) * 0.5f);

                using (_pluginInterface.UiBuilder.IconFontHandle.Push()) {
                    ImGui.TextUnformatted(starIcon);
                }
                
                ImGui.SameLine(0, 4f);
                ImGui.TextUnformatted(titleLabel);
                ImGui.PopStyleColor();

                ImGui.Spacing();

                var body = $"Hi there {testerInfo.Nickname}!～\n\n\n" + "Thank you so much for all of the feedback and the time you've contributed to helping me in making this plugin better for everyone.\n\n\n\n" + "So.. This is a lil' badge as a small token of my gratitude and appreciation, thanks lots!～";

                foreach (var paragraph in body.Split('\n')) {
                    if (paragraph.Length == 0) {
                        ImGui.Spacing();
                        continue;
                    }
                    foreach (var line in WrapText(paragraph, avail)) {
                        var width = ImGui.CalcTextSize(line).X;
                        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + Math.Max(0f, (avail - width) * 0.5f));
                        ImGui.TextUnformatted(line);
                    }
                }

                var hasMessage = !string.IsNullOrWhiteSpace(testerInfo.Message) && !testerInfo.Message.Trim().Equals("N/A", StringComparison.OrdinalIgnoreCase);

                if (hasMessage) {
                    ImGui.Spacing();
                    ImGui.Spacing();
                    ImGui.Spacing();
                    ImGui.Separator();
                    ImGui.Spacing();
                    ImGui.Spacing();

                    ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.65f, 0.65f, 0.90f, 0.90f));
                    var msg = $"\u201C{testerInfo.Message.Trim()}\u201D";
                    
                    foreach (var line in WrapText(msg, avail)) {
                        var width = ImGui.CalcTextSize(line).X;
                        
                        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + Math.Max(0f, (avail - width) * 0.5f));
                        ImGui.TextUnformatted(line);
                    }
                    
                    ImGui.PopStyleColor();
                }

                ImGui.Spacing();
                ImGui.Spacing();
                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();

                const float closeWidth = 150f;
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + Math.Max(0f, (avail - closeWidth) * 0.5f));
                if (ImGui.Button("Close", new Vector2(closeWidth, 0))) {
                    ImGui.CloseCurrentPopup();
                }

                ImGui.EndPopup();
            }
        }

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
                
                ImGui.PushTextWrapPos(0f);
                ImGui.TextWrapped("The skips contained within this category are very experimental and may carry a heavy ban risk.\n\nBy enabling any of these, you accept full responsibility for any consequences, I, the developer, have no responsibility in your choice of usages.");
                ImGui.PopTextWrapPos();
                
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

        private void DrawDevPopup() {
            if (_showDevPopup) {
                ImGui.OpenPopup("##DevPopup");
                _showDevPopup = false;
                _devPopupOpen = true;
            }

            ImGui.SetNextWindowSize(new Vector2(420, 0), ImGuiCond.Always);

            if (ImGui.BeginPopupModal("##DevPopup", ref _devPopupOpen, ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize)) {
                ImGui.Spacing();
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.40f, 0.60f, 1.0f, 1f));

                var icon = FontAwesomeIcon.Flask.ToIconString();
                float width;
                
                using (_pluginInterface.UiBuilder.IconFontHandle.Push()) {
                    width = ImGui.CalcTextSize(icon).X;
                }

                var title = " Developer Mode";
                var titleWidth = width + ImGui.CalcTextSize(title).X + 4f;
                var avail = ImGui.GetContentRegionAvail().X;
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (avail - titleWidth) * 0.5f);

                using (_pluginInterface.UiBuilder.IconFontHandle.Push()) {
                    ImGui.TextUnformatted(icon);
                }
                
                ImGui.SameLine(0, 4f);
                ImGui.TextUnformatted(title);
                ImGui.PopStyleColor();
                ImGui.Spacing();
                
                ImGui.TextWrapped("This restricted mode is reserved for the developer and authorized contributors assisting with signature hooks.\n\n" + "Warning: This menu contains isolated hooks that bypass all standard exemptions. As they are dangerous if used incorrectly, all hooks are toggled off by default and are locked behind authorization.\n\n" + "Please enter the password below to unlock the Developer Research section.");
                
                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();

                ImGui.AlignTextToFramePadding();
                ImGui.TextUnformatted("Password:");
                ImGui.SameLine();
                ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
                ImGui.InputText("##DevPassword", ref _devPasswordInput, 128, ImGuiInputTextFlags.Password);

                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();

                bool pending = _devPasswordPending;
                if (pending) ImGui.BeginDisabled();

                float cancelWidth = 140f;
                float confirmWidth = avail - cancelWidth - ImGui.GetStyle().ItemSpacing.X;

                if (ImGui.Button("Cancel", new Vector2(cancelWidth, 0))) {
                    ImGui.CloseCurrentPopup();
                }

                ImGui.SameLine();

                if (ImGui.Button(pending ? "Checking..." : "Confirm", new Vector2(confirmWidth, 0))) {
                    _devPasswordPending = true;
                    var input = _devPasswordInput;
                    
                    _ = Skippy.Instance.TryEnableDevMode(input).ContinueWith(_ => _devPasswordPending = false);
                    ImGui.CloseCurrentPopup();
                }

                if (pending) {
                    ImGui.EndDisabled();
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
            ImGui.SetCursorPosX((width - button) * 0.5f);

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
                ImGui.TextUnformatted(hasError ? "[ERROR] — Cutscene Offsets Not Found" : _config.IsEnabled ? "[ENABLED] — Click to Disable Skippy" : "[DISABLED] — Click to Enable Skippy");
            }

        }

        private void NavItem(string label, Category target, bool active, bool hasError = false, Vector4? circleColorOverride = null) {
            var height = ImGui.GetTextLineHeight() + ImGui.GetStyle().ItemSpacing.Y * 3f;
            var greyOut = !_config.IsEnabled && !(_config.AutoEnable4Man && _config.IsEnabled && target == Category.MSQRoulette);

            var iconColor = greyOut ? new Vector4(0.5f, 0.5f, 0.5f, 0.4f) : circleColorOverride ?? (!active ? new Vector4(0.70f, 0.20f, 0.20f, 1.0f) : hasError ? new Vector4(1.0f, 0.55f, 0.10f, 1.0f) : new Vector4(0.20f, 0.80f, 0.20f, 1.0f));
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

        private void IconNavItem(string label, Category target, FontAwesomeIcon faIcon, Vector4 iconColor, bool colorText = false) {
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
            drawList.AddText(ImGui.GetFont(), ImGui.GetFontSize(), new Vector2(textX, textY), colorText && !greyOut ? ImGui.ColorConvertFloat4ToU32(iconColor) : ImGui.GetColorU32(ImGuiCol.Text), label);

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
            
            if (ImGui.IsItemClicked()) {
                value = !value;
                changed = true;
            }
            
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

        internal static void WrappedCenteredText(string text, float wrapWidth) {
            var avail = ImGui.GetContentRegionAvail().X;
            var baseX = ImGui.GetCursorPosX();

            foreach (var line in WrapText(text, wrapWidth)) {
                var lineWidth = ImGui.CalcTextSize(line).X;
                var offsetX = (avail - lineWidth) * 0.5f;
                
                if (offsetX < 0f) {
                    offsetX = 0f;
                }
                
                ImGui.SetCursorPosX(baseX + offsetX);
                ImGui.TextUnformatted(line);
            }
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
        
        private static IEnumerable<string> WrapText(string text, float maxWidth) {
            var words = text.Split(' ');
            var line  = new StringBuilder();

            foreach (var word in words) {
                var test = line.Length == 0 ? word : line + " " + word;
                
                if (ImGui.CalcTextSize(test).X > maxWidth && line.Length > 0) {
                    yield return line.ToString();
                    line.Clear();
                    line.Append(word);
                } else {
                    if (line.Length > 0) {
                        line.Append(' ');
                    }
                    
                    line.Append(word);
                }
            }

            if (line.Length > 0) {
                yield return line.ToString();
            }
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