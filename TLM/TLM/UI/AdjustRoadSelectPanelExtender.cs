namespace TrafficManager.UI {
    using System;
    using System.Collections.Generic;
    using JetBrains.Annotations;

    using ColossalFramework.UI;
    using UnityEngine;
    using ColossalFramework;
    using ICities;
    using CSUtil.Commons;

    using Textures;
    using Util;


    public class AdjustRoadSelectPanelExtender : MonoBehaviour {
        public static AdjustRoadSelectPanelExtender Instance { get; private set; } = null;
        public AdjustRoadSelectPanelExtender(): base() { Instance = this; }

        public enum FunctionMode {
            Clear=0,
            Stop,
            Yeild,
            MiddleBarrier,
            Rabout,
        }

        private FunctionMode _function = FunctionMode.Clear;
        public FunctionMode Function {
            get => _function;
            set {
                _function = value;
                Refresh();
            }
        }

        private UICheckBox priorityRoadToggle = null;
        public void HidePriorityRoadToggle() {
            if (priorityRoadToggle == null)
                return;
            Debug.Log($"priority toggle => {priorityRoadToggle.name}");
            if (priorityRoadToggle.isVisible) {
                priorityRoadToggle.isVisible = false;
            }
            priorityRoadToggle.Disable();
        }

        //public class Threading : ThreadingExtensionBase {
        //    public override void OnAfterSimulationFrame() {
        //        //AdjustRoadSelectPanelExtender.Instance?.HidePriorityRoadToggle();
        //    }
        //}

        public IList<PanelExt> panels;
        public void Start() {
            Debug.Log("POINT A1");
            panels = new List<PanelExt>();

            RoadWorldInfoPanel roadWorldInfoPanel = UIView.library.Get<RoadWorldInfoPanel>("RoadWorldInfoPanel");
            if (roadWorldInfoPanel != null) {
                priorityRoadToggle = roadWorldInfoPanel.component.Find<UICheckBox>("PriorityRoadCheckbox");
                PanelExt panel = AddPanel(roadWorldInfoPanel.component);
                panel.relativePosition += new Vector3(0, -15f);
                //roadWorldInfoPanel.component.eventVisibilityChanged +=
                //    (_, __) => HidePriorityRoadToggle();
                priorityRoadToggle.eventVisibilityChanged +=
                    (_, __) => HidePriorityRoadToggle();
                panels.Add(panel);
            }

            UIPanel roadAdjustPanel = UIView.Find<UIPanel>("AdjustRoad");
            if (roadAdjustPanel != null) {
                //UILabel roadSelectLabel = roadAdjustPanel.Find<UILabel>("Label");
                //UILabel roadSelectLegend = roadAdjustPanel.Find<UILabel>("LegendLabel");
                //UISprite roadSelectSprite = roadAdjustPanel.Find<UISprite>("Sprite");
                //roadSelectLabel.isVisible = false;
                //roadSelectLegend.isVisible = false;
                //roadSelectSprite.isVisible = false;
                PanelExt panel = AddPanel(roadAdjustPanel);
                panels.Add(panel);
            }

            RoadSelection.Instance.OnChanged += ()=>Refresh();

Debug.Log("POINT A2");
        }

        protected PanelExt AddPanel(UIComponent container) {
            UIView uiview = UIView.GetAView();
            PanelExt panel = uiview.AddUIComponent(typeof(PanelExt)) as PanelExt;
            panel.adjustRoadSelectPanelExtender = this;
            panel.width = 210;
            panel.height = 50;
            panel.AlignTo(container, UIAlignAnchor.BottomLeft);
            panel.relativePosition += new Vector3(70, 0);
            panels.Add(panel);
            return panel;
        }

        public void OnDestroy() {
            Instance = null;
            priorityRoadToggle = null;
            if (panels == null) {
                return;
            }

            foreach (UIPanel panel in panels) {
                Destroy(panel);
            }
        }

        public void Refresh() {
            foreach(var panel in panels) {
                panel.Refresh();
            }
        }


        public class PanelExt : UIPanel {
            public void Refresh() {
                foreach(var button in buttons) {
                    button.UpdateProperties();
                }
            }

            public AdjustRoadSelectPanelExtender adjustRoadSelectPanelExtender;
            //UIButton Clear, TrafficLight, Yeild, Stop , RightOnly, RoundAbout;
            public IList<ButtonExt> buttons;
            public void Start() {
                Debug.Log("POINT B1");
                //backgroundSprite = "GenericPanel"; //TODO remove
                //opacity = 0.5f; // TODO remove
                autoLayout = true;
                autoLayoutDirection = LayoutDirection.Horizontal;
                padding = new RectOffset(5, 5, 5, 5);
                autoLayoutPadding = new RectOffset(5, 5, 5, 5);

                buttons = new List<ButtonExt>();
                buttons.Add(AddUIComponent<ClearButtton>());
                buttons.Add(AddUIComponent<StopButtton>());
                buttons.Add(AddUIComponent<YeildButtton>());
                buttons.Add(AddUIComponent<AvneueButtton>());
                buttons.Add(AddUIComponent<RboutButtton>());

                Debug.Log("POINT B2");
            }

            public void OnDestroy() {
                if (buttons == null) {
                    return;
                }

                foreach (UIButton button in buttons) {
                    Destroy(button);
                }
            }

            public class ClearButtton : ButtonExt {
                public override string Tooltip => Translation.Menu.Get("Tooltip:Clear");
                public override FunctionMode Function => FunctionMode.Clear;
                public override bool Active => false;
                public override void OnActivate() { }
                public override void OnDeactivate() { throw new Exception("Unreachable code"); }
            }
            public class StopButtton : ButtonExt {
                public override string Tooltip => Translation.Menu.Get("Tooltip:Stop entry");
                public override FunctionMode Function => FunctionMode.Stop;
                public override void OnActivate() { }
                public override void OnDeactivate() { }

            }
            public class YeildButtton : ButtonExt {
                public override string Tooltip => Translation.Menu.Get("Tooltip:Yeil entry");
                public override FunctionMode Function => FunctionMode.Yeild;
                public override void OnActivate() { }
                public override void OnDeactivate() { }

            }
            public class AvneueButtton : ButtonExt {
                public override string Tooltip => Translation.Menu.Get("Tooltip:Avenue");
                public override FunctionMode Function => FunctionMode.MiddleBarrier;
                public override void OnActivate() { }
                public override void OnDeactivate() { }

            }
            public class RboutButtton : ButtonExt {
                public override string Tooltip => Translation.Menu.Get("Tooltip:Roundabout");
                public override FunctionMode Function => FunctionMode.Rabout;
                public override void OnActivate() { }
                public override void OnDeactivate() { }
                public override bool IsDisabled =>
                    RoundaboutMassEdit.IsRabout(RoadSelection.Instance.Selection, semi:false);
            }

            public abstract class ButtonExt : LinearSpriteButton {
                public override void Start() {
                    base.Start();
                    width = Width;
                    height = Height;
                    Debug.Log("POINT C");
                }

                public bool IsEnabled => RoadSelection.Instance.Length > 0;

                public override void HandleClick(UIMouseEventParameter p) { }

                public abstract void OnActivate();
                public abstract void OnDeactivate();

                protected override void OnClick(UIMouseEventParameter p) {
                    if (!Active) {
                        Root.Function = this.Function;
                        OnActivate();
                    } else {
                        Root.Function = FunctionMode.Clear;
                        OnDeactivate();
                    }
                }

                public AdjustRoadSelectPanelExtender Root => AdjustRoadSelectPanelExtender.Instance;
                public override bool CanActivate() => true;

                public override bool Active => Root.Function == this.Function;

                public override bool CanDisable => false;

                public override bool IsDisabled => RoadSelection.Instance.Length == 0;

                public abstract FunctionMode Function { get; }

                public override string FunctionName => Function.ToString();

                public override string[] FunctionNames => Enum.GetNames(typeof(FunctionMode));

                public override string ButtonName => "RoadQuickEdit_" + this.GetType().ToString();

                public override Texture2D AtlasTexture => TextureResources.RoadQuickEditButtons;

                public override bool Visible => true;

                public override int Width => 30;

                public override int Height => 30;
            } // end class QuickEditButton
        } // end class PanelExt
    } // end AdjustRoadSelectPanelExt
} //end namesapce
