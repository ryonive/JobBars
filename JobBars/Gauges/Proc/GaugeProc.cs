﻿using JobBars.Data;
using JobBars.Helper;
using JobBars.UI;
using System;
using System.Linq;
using System.Collections.Generic;

namespace JobBars.Gauges {
    public struct GaugeProcProps {
        public bool ShowText;
        public Proc[] Procs;
        public bool NoSoundOnProc;
    }

    public class Proc {
        public readonly string Name;
        public readonly Item Trigger;

        public int Idx = 0;
        public int Order;
        public ElementColor Color;
        public bool Active = true;

        public Proc(string name, BuffIds buff, ElementColor color) : this(name, new Item(buff), color) { }
        public Proc(string name, ActionIds action, ElementColor color) : this(name, new Item(action), color) { }
        public Proc(string name, Item trigger, ElementColor color) {
            Name = name;
            Trigger = trigger;
            Color = JobBars.Config.GaugeProcColor.Get(Name, color);
            Order = JobBars.Config.GaugeProcOrder.Get(Name);
        }
    }

    public class GaugeProc : Gauge {
        private readonly Proc[] Procs;
        private bool ProcsShowText;
        private bool ProcSound;

        private readonly int Size;
        private GaugeState State = GaugeState.Inactive;

        public GaugeProc(string name, GaugeProcProps props) : base(name) {
            Procs = props.Procs;
            Size = Procs.Length;
            ProcsShowText = JobBars.Config.GaugeShowText.Get(Name, props.ShowText);
            ProcSound = JobBars.Config.GaugeProgressSound.Get(Name, !props.NoSoundOnProc);
            RefreshIdx();
        }

        private void RefreshIdx() {
            var idx = 0;
            foreach (var proc in Procs.OrderBy(x => x.Order)) {
                proc.Idx = idx++;
            }
        }

        protected override void LoadUI_() {
            if (UI is UIDiamond diamond) {
                diamond.SetMaxValue(Size);
            }
            for (int idx = 0; idx < Size; idx++) {
                SetValue(idx, false);
            }

            State = GaugeState.Inactive;
            ResetProcActive();
        }

        protected override void ApplyUIVisual_() {
            if (UI is UIDiamond diamond) {
                foreach (var proc in Procs) diamond.SetColor(proc.Color, proc.Idx);
                diamond.SetTextVisible(ProcsShowText);
            }
        }

        private void ResetProcActive() {
            foreach (var proc in Procs) proc.Active = true;
        }

        public unsafe override void Tick() {
            var playSound = false;
            var procActiveCount = 0;
            foreach (var proc in Procs) {
                bool procActive;

                if (proc.Trigger.Type == ItemType.Buff) {
                    SetValue(proc.Idx, procActive = UIHelper.PlayerStatus.TryGetValue(proc.Trigger, out var buff), buff.RemainingTime);
                }
                else {
                    var recastActive = UIHelper.GetRecastActive(proc.Trigger.Id, out _);
                    SetValue(proc.Idx, procActive = !recastActive);
                }

                if (procActive && !proc.Active) playSound = true;
                if (procActive) procActiveCount++;
                proc.Active = procActive;
            }

            if (playSound && ProcSound) UIHelper.PlaySeComplete();
            State = procActiveCount == 0 ? GaugeState.Inactive : GaugeState.Active;
        }

        protected override bool GetActive() => State != GaugeState.Inactive;

        private void SetValue(int idx, bool value, float duration = -1) {
            if (UI is UIDiamond diamond) {
                if (value) {
                    diamond.SelectPart(idx);
                    if (ProcsShowText) {
                        diamond.SetText(idx, duration >= 0 ? ((int)Math.Round(duration)).ToString() : "");
                    }
                }
                else diamond.UnselectPart(idx);
            }
        }

        public override void ProcessAction(Item action) { }

        protected override int GetHeight() => UI.GetHeight(0);
        protected override int GetWidth() => UI.GetWidth(Size);
        public override GaugeVisualType GetVisualType() => GaugeVisualType.Diamond;

        protected override void DrawGauge(string _ID, JobIds job) {
            if (JobBars.Config.GaugeShowText.Draw($"Show Text{_ID}", Name, ProcsShowText, out var newProcsShowText)) {
                ProcsShowText = newProcsShowText;
                ApplyUIVisual();
                JobBars.GaugeManager.UpdatePositionScale(job); // procs with text are taller than without, so update positions
            }

            if (JobBars.Config.GaugeProgressSound.Draw($"Play Sound on Proc{_ID}", Name, ProcSound, out var newProcSound)) {
                ProcSound = newProcSound;
            }

            foreach (var proc in Procs) {
                if (JobBars.Config.GaugeProcOrder.Draw($"Order ({proc.Name})", proc.Name, proc.Order, out var newOrder)) {
                    proc.Order = newOrder;
                    RefreshIdx();
                    JobBars.GaugeManager.ResetJob(job);
                }

                if (JobBars.Config.GaugeProcColor.Draw($"Color ({proc.Name})", proc.Name, proc.Color, out var newColor)) {
                    proc.Color = newColor;
                    JobBars.GaugeManager.ResetJob(job);
                }
            }
        }
    }
}
