﻿using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.Graphics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using JobBars.GameStructs;
using JobBars.Helper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JobBars.UI {
    public struct UIIconProps {
        public bool IsTimer;
        public bool IsGCD; // only matters with Timer
        public bool UseCombo;
        public bool UseBorder;
    }

    public unsafe class UIIconManager {
        private class Icon {
            private enum IconState {
                None,
                TimerRunning,
                TimerDone,
                BuffRunning
            }

            public readonly uint AdjustedId;
            public readonly uint SlotId;
            public readonly int HotbarIdx;
            public readonly int SlotIdx;
            public AtkComponentNode* Component;

            private AtkResNode* OriginalOverlay;
            private AtkImageNode* OriginalImage;

            private AtkImageNode* Border;
            private AtkImageNode* Circle;
            private AtkImageNode* Ring;
            private AtkTextNode* Text;
            private AtkTextNode* BigText;
            private IconState State = IconState.None;

            private readonly bool IsTimer;
            private readonly bool IsGCD;
            private readonly bool UseCombo;
            private readonly bool UseBorder;

            private bool Dimmed = false;

            public Icon(uint adjustedId, uint slotId, int hotbarIdx, int slotIdx, AtkComponentNode* component, UIIconProps props) {
                AdjustedId = adjustedId;
                SlotId = slotId;
                HotbarIdx = hotbarIdx;
                SlotIdx = slotIdx;
                Component = component;

                var nodeList = Component->Component->UldManager.NodeList;
                OriginalOverlay = nodeList[1];

                IsTimer = props.IsTimer;
                UseCombo = props.UseCombo;
                IsGCD = props.IsGCD;
                UseBorder = props.UseBorder;

                OriginalImage = (AtkImageNode*)nodeList[0];
                var originalBorder = (AtkImageNode*)nodeList[4];
                var originalCD = (AtkImageNode*)nodeList[5];
                var originalCircle = (AtkImageNode*)nodeList[7];

                uint nodeIdx = 200;

                Border = UIHelper.CleanAlloc<AtkImageNode>();
                Border->Ctor();
                Border->AtkResNode.NodeID = nodeIdx++;
                Border->AtkResNode.Type = NodeType.Image;
                Border->AtkResNode.X = -2;
                Border->AtkResNode.Width = 48;
                Border->AtkResNode.Height = 48;
                Border->AtkResNode.Flags = 8243;
                Border->AtkResNode.Flags_2 = 1;
                Border->AtkResNode.Flags_2 |= 4;
                Border->WrapMode = 1;
                Border->PartId = 0;
                Border->PartsList = originalBorder->PartsList;

                Circle = UIHelper.CleanAlloc<AtkImageNode>(); // for timer
                Circle->Ctor();
                Circle->AtkResNode.NodeID = nodeIdx++;
                Circle->AtkResNode.Type = NodeType.Image;
                Circle->AtkResNode.X = 0;
                Circle->AtkResNode.Y = 2;
                Circle->AtkResNode.Width = 44;
                Circle->AtkResNode.Height = 46;
                Circle->AtkResNode.Flags = 8243;
                Circle->AtkResNode.Flags_2 = 1;
                Circle->AtkResNode.Flags_2 |= 4;
                Circle->WrapMode = 1;
                Circle->PartId = 0;
                Circle->PartsList = originalCD->PartsList;

                Ring = UIHelper.CleanAlloc<AtkImageNode>(); // for timer
                Ring->Ctor();
                Ring->AtkResNode.NodeID = nodeIdx++;
                Ring->AtkResNode.Type = NodeType.Image;
                Ring->AtkResNode.X = 0;
                Ring->AtkResNode.Y = 2;
                Ring->AtkResNode.Width = 44;
                Ring->AtkResNode.Height = 46;
                Ring->AtkResNode.Flags = 8243;
                Ring->AtkResNode.Flags_2 = 1;
                Ring->AtkResNode.Flags_2 |= 4;
                Ring->WrapMode = 1;
                Ring->PartId = 0;
                Ring->PartsList = originalCircle->PartsList;

                Text = UIHelper.CleanAlloc<AtkTextNode>();
                Text->Ctor();
                Text->AtkResNode.NodeID = nodeIdx++;
                Text->AtkResNode.Type = NodeType.Text;
                Text->AtkResNode.X = 1;
                Text->AtkResNode.Y = 37;
                Text->AtkResNode.Width = 48;
                Text->AtkResNode.Height = 12;
                Text->AtkResNode.Flags = 8243;
                Text->AtkResNode.Flags_2 = 1;
                Text->AtkResNode.Flags_2 |= 4;
                Text->LineSpacing = 12;
                Text->AlignmentFontType = 3;
                Text->FontSize = 12;
                Text->TextFlags = 16;
                Text->TextColor = new ByteColor { R = 255, G = 255, B = 255, A = 255 };
                Text->EdgeColor = new ByteColor { R = 51, G = 51, B = 51, A = 255 };
                Text->SetText("");

                BigText = UIHelper.CleanAlloc<AtkTextNode>();
                BigText->Ctor();
                BigText->AtkResNode.NodeID = nodeIdx++;
                BigText->AtkResNode.Type = NodeType.Text;
                BigText->AtkResNode.X = 2;
                BigText->AtkResNode.Y = 3;
                BigText->AtkResNode.Width = 40;
                BigText->AtkResNode.Height = 40;
                BigText->AtkResNode.Flags = 8243;
                BigText->AtkResNode.Flags_2 = 1;
                BigText->AtkResNode.Flags_2 |= 4;
                BigText->LineSpacing = 40;
                BigText->AlignmentFontType = 20;
                BigText->FontSize = 16;
                BigText->TextFlags = 16;
                BigText->TextColor = new ByteColor { R = 255, G = 255, B = 255, A = 255 };
                BigText->EdgeColor = new ByteColor { R = 51, G = 51, B = 51, A = 255 };
                BigText->SetText("");

                var macroIcon = nodeList[15];
                var rootNode = (AtkResNode*)Component;

                Border->AtkResNode.ParentNode = rootNode;
                Circle->AtkResNode.ParentNode = rootNode;
                Ring->AtkResNode.ParentNode = rootNode;
                Text->AtkResNode.ParentNode = rootNode;

                UIHelper.Link(OriginalOverlay, (AtkResNode*)Circle);
                UIHelper.Link((AtkResNode*)Circle, (AtkResNode*)Ring);
                UIHelper.Link((AtkResNode*)Ring, (AtkResNode*)Border);
                UIHelper.Link((AtkResNode*)Border, (AtkResNode*)BigText);
                UIHelper.Link((AtkResNode*)BigText, (AtkResNode*)Text);
                UIHelper.Link((AtkResNode*)Text, macroIcon);

                Component->Component->UldManager.UpdateDrawNodeList();

                if (IsTimer) UIHelper.Hide(OriginalOverlay);

                UIHelper.Hide(Circle);
                UIHelper.Hide(Ring);
                UIHelper.Hide(Text);
                UIHelper.Hide(BigText);
            }

            public void SetProgress(float current, float max) {
                if (IsTimer) SetTimerProgress(current, max);
                else SetBuffProgress(current);
            }

            public void SetDone() {
                if (IsTimer) SetTimerDone();
                else SetBuffDone();
            }

            // ======== FOR DoT TIMERS ========

            private void SetTimerProgress(float current, float max) {
                if (State == IconState.TimerDone && current <= 0) return;
                State = IconState.TimerRunning;

                UIHelper.Show(Text);
                Text->SetText(((int)Math.Round(current)).ToString());

                UIHelper.Show(IsGCD ? Ring : Circle);
                (IsGCD ? Ring : Circle)->PartId = (ushort)(80 - (float)(current / max) * 80);
                if (IsGCD) {
                    JobBars.IconBuilder.AddIconOverride(new IntPtr(OriginalImage));
                    SetDimmed(true);
                }
            }

            private void SetTimerDone() {
                State = IconState.TimerDone;
                UIHelper.Hide(Text);

                UIHelper.Hide(IsGCD ? Ring : Circle);
                if (IsGCD) {
                    JobBars.IconBuilder.RemoveIconOverride(new IntPtr(OriginalImage));
                    SetDimmed(false);
                }
            }

            // ====== FOR BUFFS =============

            private void SetBuffProgress(float current) {
                if (State != IconState.BuffRunning) {
                    State = IconState.BuffRunning;
                    UIHelper.Hide(OriginalOverlay);
                    UIHelper.Show(BigText);
                }
                BigText->SetText(((int)Math.Round(current)).ToString());
            }

            private void SetBuffDone() {
                if (State == IconState.None) return;
                State = IconState.None;

                UIHelper.Hide(BigText);
                UIHelper.Show(OriginalOverlay);
            }

            // ======================

            private void SetDimmed(bool dimmed) {
                Dimmed = dimmed;
                SetDimmed(OriginalImage, dimmed);
            }

            public static void SetDimmed(AtkImageNode* image, bool dimmed) {
                var val = (byte)(dimmed ? 50 : 100);
                image->AtkResNode.MultiplyRed = val;
                image->AtkResNode.MultiplyRed_2 = val;
                image->AtkResNode.MultiplyGreen = val;
                image->AtkResNode.MultiplyGreen_2 = val;
                image->AtkResNode.MultiplyBlue = val;
                image->AtkResNode.MultiplyBlue_2 = val;
            }

            public void Tick(float dashPercent, float gcdPercent, bool border) {
                var useBorder = (UseCombo && border) || (UseBorder && (State == IconState.TimerDone || State == IconState.BuffRunning));

                Border->PartId = !useBorder ? (ushort)0 : (ushort)(6 + dashPercent * 7);

                if(IsTimer && IsGCD) {
                    Circle->PartId = (ushort)(gcdPercent * 80);
                    UIHelper.SetVisibility(Circle, gcdPercent > 0);
                }
            }

            public void Dispose() {
                var rootNode = (AtkResNode*)Component;

                UIHelper.Link(OriginalOverlay, Text->AtkResNode.PrevSiblingNode);
                Component->Component->UldManager.UpdateDrawNodeList();

                UIHelper.Show(OriginalOverlay);
                JobBars.IconBuilder.RemoveIconOverride(new IntPtr(OriginalImage));
                if(Dimmed) SetDimmed(false);

                if (Border != null) {
                    Border->AtkResNode.Destroy(true);
                    Border = null;
                }

                if (Circle != null) {
                    Circle->AtkResNode.Destroy(true);
                    Circle = null;
                }

                if (Ring != null) {
                    Ring->AtkResNode.Destroy(true);
                    Ring = null;
                }

                if (Text != null) {
                    Text->AtkResNode.Destroy(true);
                    Text = null;
                }

                if (BigText != null) {
                    BigText->AtkResNode.Destroy(true);
                    BigText = null;
                }

                Component = null;
                OriginalOverlay = null;
                OriginalImage = null;
            }
        }

        // ==================

        private readonly string[] AllActionBars = {
            "_ActionBar",
            "_ActionBar01",
            "_ActionBar02",
            "_ActionBar03",
            "_ActionBar04",
            "_ActionBar05",
            "_ActionBar06",
            "_ActionBar07",
            "_ActionBar08",
            "_ActionBar09",
            "_ActionCross",
            "_ActionDoubleCrossL",
            "_ActionDoubleCrossR",
        };
        private static readonly int MILLIS_LOOP = 250;

        private readonly Dictionary<uint, UIIconProps> IconConfigs = new();
        private readonly List<Icon> Icons = new();
        private readonly HashSet<IntPtr> IconOverride = new();

        public UIIconManager() {
        }

        public void Setup(List<uint> triggers, UIIconProps props) {
            if (triggers == null) return;

            foreach(var trigger in triggers) {
                IconConfigs[trigger] = props;
            }
        }

        public void SetProgress(List<uint> triggers, float current, float max) {
            foreach (var icon in Icons) {
                if (!triggers.Contains(icon.AdjustedId)) continue;
                icon.SetProgress(current, max);
            }
        }

        public void SetDone(List<uint> triggers) {
            foreach (var icon in Icons) {
                if (!triggers.Contains(icon.AdjustedId)) continue;
                icon.SetDone();
            }
        }

        public void Tick() {
            var time = DateTime.Now;
            var millis = time.Second * 1000 + time.Millisecond;
            var percent = (float)(millis % MILLIS_LOOP) / MILLIS_LOOP;
            var gcdPercent = UIHelper.GetGCD();

            var hotbarData = UIHelper.GetHotbarUI();
            if (hotbarData == null) return;

            HashSet<Icon> FoundIcons = new();

            for (var hotbarIndex = 0; hotbarIndex < AllActionBars.Length; hotbarIndex++) {
                var hotbar = hotbarData->Hotbars[hotbarIndex];

                var actionBar = (AddonActionBarBase*)AtkStage.GetSingleton()->RaptureAtkUnitManager->GetAddonByName(AllActionBars[hotbarIndex]);
                if (actionBar == null || actionBar->ActionBarSlotsAction == null) continue;

                for (var slotIndex = 0; slotIndex < actionBar->HotbarSlotCount; slotIndex++) {
                    var slotData = hotbar[slotIndex];
                    if (slotData.Type != HotbarSlotStructType.Action) continue;

                    var action = UIHelper.GetAdjustedAction(slotData.ActionId);

                    if (!IconConfigs.TryGetValue(action, out var props)) continue; // not looking for this action id

                    var found = false;
                    foreach (var icon in Icons) {
                        if (icon.HotbarIdx != hotbarIndex || icon.SlotIdx != slotIndex) continue;
                        if (slotData.ActionId != icon.SlotId) break; // action changed, just ignore it

                        // found existing icon which matches
                        found = true;
                        FoundIcons.Add(icon);
                        icon.Tick(percent, gcdPercent, slotData.YellowBorder);
                        break;
                    }
                    if (found) continue; // already found an icon, don't need to create a new one

                    var slot = actionBar->ActionBarSlotsAction[slotIndex];
                    var newIcon = new Icon(action, slotData.ActionId, hotbarIndex, slotIndex, slot.Icon, props);
                    FoundIcons.Add(newIcon);
                    Icons.Add(newIcon);
                }
            }

            foreach (var icon in Icons.Where(x => !FoundIcons.Contains(x))) icon.Dispose();
            Icons.RemoveAll(x => !FoundIcons.Contains(x));
        }

        public void AddIconOverride(IntPtr icon) {
            IconOverride.Add(icon);
        }

        public void RemoveIconOverride(IntPtr icon) {
            IconOverride.Remove(icon);
        }

        public void ProcessIconOverride(IntPtr icon) {
            if (icon == IntPtr.Zero) return;
            if(IconOverride.Contains(icon)) {
                var image = (AtkImageNode*)icon;
                Icon.SetDimmed(image, true);
            }
        }

        public void ResetUI() {
            Icons.ForEach(x => x.Dispose());
            Icons.Clear();
        }

        public void Reset() {
            IconConfigs.Clear();
            ResetUI();
        }

        public void Dispose() {
            IconOverride.Clear();
            Reset();
        }
    }
}
