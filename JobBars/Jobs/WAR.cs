using JobBars.Atk;
using JobBars.Buffs;
using JobBars.Cooldowns;
using JobBars.Cursors;
using JobBars.Data;
using JobBars.Gauges;
using JobBars.Gauges.Stacks;
using JobBars.Gauges.Timer;
using JobBars.Helper;
using JobBars.Icons;

namespace JobBars.Jobs {
    public static class WAR {
        public static GaugeConfig[] Gauges => [
            new GaugeStacksConfig(AtkHelper.Localize(BuffIds.InnerRelease), GaugeVisualType.Diamond, new GaugeStacksProps {
                MaxStacks = 3,
                Triggers = [
                    new Item(BuffIds.InnerRelease)
                ],
                Color = AtkColor.Orange
            }),
            new GaugeTimerConfig(AtkHelper.Localize(BuffIds.SurgingTempest), GaugeVisualType.Bar, new GaugeSubTimerProps {
                MaxDuration = 60,
                DefaultDuration = 30,
                Color = AtkColor.Red,
                Triggers = [
                    new Item(BuffIds.SurgingTempest)
                ]
            })
        ];

        public static BuffConfig[] Buffs => [
            new BuffConfig(AtkHelper.Localize(BuffIds.InnerRelease), new BuffProps {
                Duration = 15,
                CD = 60,
                Icon = ActionIds.InnerRelease,
                Color = AtkColor.Orange,
                Triggers = [new Item(BuffIds.InnerRelease)]
            })
        ];

        public static Cursor Cursors => new( JobIds.WAR, CursorType.None, CursorType.GCD );

        public static CooldownConfig[] Cooldowns => [
            new CooldownConfig(AtkHelper.Localize(ActionIds.Holmgang), new CooldownProps {
                Icon = ActionIds.Holmgang,
                Duration = 10,
                CD = 240,
                Triggers = [new Item(ActionIds.Holmgang)]
            }),
            new CooldownConfig($"{AtkHelper.Localize(ActionIds.Reprisal)} ({AtkHelper.Localize(JobIds.WAR)})", new CooldownProps {
                Icon = ActionIds.Reprisal,
                Duration = 15,
                CD = 60,
                Triggers = [new Item(ActionIds.Reprisal)]
            }),
            new CooldownConfig(AtkHelper.Localize(ActionIds.ShakeItOff), new CooldownProps {
                Icon = ActionIds.ShakeItOff,
                Duration = 15,
                CD = 90,
                Triggers = [new Item(ActionIds.ShakeItOff)]
            }),
            new CooldownConfig(AtkHelper.Localize(ActionIds.Bloodwhetting), new CooldownProps {
                Icon = ActionIds.Bloodwhetting,
                Duration = 6,
                CD = 25,
                Triggers = [
                    new Item(ActionIds.NascentFlash),
                    new Item(ActionIds.RawIntuition),
                    new Item(ActionIds.Bloodwhetting)
                ]
            })
        ];

        public static IconReplacer[] Icons => new[] {
            new IconBuffReplacer(AtkHelper.Localize(BuffIds.SurgingTempest), new IconBuffProps {
                IsTimer = true,
                Icons = [ActionIds.StormsEye],
                Triggers = [
                    new IconBuffTriggerStruct { Trigger = new Item(BuffIds.SurgingTempest), Duration = 60 }
                ]
            }),
            new IconBuffReplacer($"{AtkHelper.Localize(ActionIds.Rampart)} ({AtkHelper.Localize(JobIds.WAR)})", new IconBuffProps {
                Icons = [ActionIds.Rampart],
                Triggers = [
                    new IconBuffTriggerStruct { Trigger = new Item(BuffIds.Rampart), Duration = 20 }
                ]
            }),
            new IconBuffReplacer(AtkHelper.Localize(BuffIds.Damnation), new IconBuffProps {
                Icons = [
                    ActionIds.Vengeance,
                    ActionIds.Damnation
                ],
                Triggers = [
                    new IconBuffTriggerStruct { Trigger = new Item(BuffIds.Vengeance), Duration = 15 },
                    new IconBuffTriggerStruct { Trigger = new Item(BuffIds.Damnation), Duration = 15 }
                ]
            }),
            new IconBuffReplacer(AtkHelper.Localize(BuffIds.Bloodwhetting), new IconBuffProps {
                Icons = [
                    ActionIds.RawIntuition,
                    ActionIds.Bloodwhetting
                ],
                Triggers = [
                    new IconBuffTriggerStruct { Trigger = new Item(BuffIds.RawIntuition), Duration = 6 },
                    new IconBuffTriggerStruct { Trigger = new Item(BuffIds.Bloodwhetting), Duration = 8 }
                ]
            }),
            new IconBuffReplacer($"{AtkHelper.Localize(ActionIds.ArmsLength)} ({AtkHelper.Localize(JobIds.WAR)})", new IconBuffProps {
                Icons = [ActionIds.ArmsLength],
                Triggers = [
                    new IconBuffTriggerStruct { Trigger = new Item(BuffIds.ArmsLength), Duration = 6 }
                ]
            }),
            new IconBuffReplacer(AtkHelper.Localize(BuffIds.Holmgang), new IconBuffProps {
                Icons = [ActionIds.Holmgang],
                Triggers = [
                    new IconBuffTriggerStruct { Trigger = new Item(BuffIds.Holmgang), Duration = 10 }
                ]
            }),
            new IconBuffReplacer(AtkHelper.Localize(BuffIds.ThrillOfBattle), new IconBuffProps {
                Icons = [ActionIds.ThrillOfBattle],
                Triggers = [
                    new IconBuffTriggerStruct { Trigger = new Item(BuffIds.ThrillOfBattle), Duration = 10 }
                ]
            })
        };

        public static bool MP => false;

        public static float[] MP_SEGMENTS => null;
    }
}
