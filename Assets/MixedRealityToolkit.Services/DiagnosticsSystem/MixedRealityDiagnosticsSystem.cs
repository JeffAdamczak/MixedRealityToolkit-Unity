﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using UnityEngine.EventSystems;

namespace Microsoft.MixedReality.Toolkit.Diagnostics
{
    /// <summary>
    /// The default implementation of the <see cref="Microsoft.MixedReality.Toolkit.Diagnostics.IMixedRealityDiagnosticsSystem"/>
    /// </summary>
    [DocLink("https://microsoft.github.io/MixedRealityToolkit-Unity/Documentation/Diagnostics/DiagnosticsSystemGettingStarted.html")]
    public class MixedRealityDiagnosticsSystem : BaseCoreSystem, IMixedRealityDiagnosticsSystem
    {
        public MixedRealityDiagnosticsSystem(
            IMixedRealityServiceRegistrar registrar,
            MixedRealityDiagnosticsProfile profile) : base(registrar, profile)
        { }

        /// <summary>
        /// The parent object under which all visualization game objects will be placed.
        /// </summary>
        private GameObject diagnosticVisualizationParent = null;

        /// <summary>
        /// Creates the diagnostic visualizations and parents them so that the scene hierarchy does not get overly cluttered.
        /// </summary>
        private void CreateVisualizations()
        {
            diagnosticVisualizationParent = new GameObject("Diagnostics");
            diagnosticVisualizationParent.AddComponent<DiagnosticsSystemVoiceControls>();
            MixedRealityPlayspace.AddChild(diagnosticVisualizationParent.transform);
            diagnosticVisualizationParent.SetActive(ShowDiagnostics);

            // visual profiler settings
            visualProfiler = diagnosticVisualizationParent.AddComponent<MixedRealityToolkitVisualProfiler>();
            visualProfiler.WindowParent = diagnosticVisualizationParent.transform;
            visualProfiler.IsVisible = ShowProfiler;
            visualProfiler.FrameInfoVisible = ShowFrameInfo;
            visualProfiler.MemoryStatsVisible = ShowMemoryStats;
            visualProfiler.FrameSampleRate = FrameSampleRate;
            visualProfiler.WindowAnchor = WindowAnchor;
            visualProfiler.WindowOffset = WindowOffset;
            visualProfiler.WindowScale = WindowScale;
            visualProfiler.WindowFollowSpeed = WindowFollowSpeed;
        }

        private MixedRealityToolkitVisualProfiler visualProfiler = null;

        #region IMixedRealityService

        /// <inheritdoc />
        public override void Initialize()
        {
            if (!Application.isPlaying) { return; }

            MixedRealityDiagnosticsProfile profile = ConfigurationProfile as MixedRealityDiagnosticsProfile;
            if (profile == null) { return; }

            eventData = new DiagnosticsEventData(EventSystem.current);

            // Apply profile settings
            ShowDiagnostics = profile.ShowDiagnostics;
            ShowProfiler = profile.ShowProfiler;
            ShowFrameInfo = profile.ShowFrameInfo;
            ShowMemoryStats = profile.ShowMemoryStats;
            FrameSampleRate = profile.FrameSampleRate;
            WindowAnchor = profile.WindowAnchor;
            WindowOffset = profile.WindowOffset;
            WindowScale = profile.WindowScale;
            WindowFollowSpeed = profile.WindowFollowSpeed;

            CreateVisualizations();
        }

        /// <inheritdoc />
        public override void Destroy()
        {
            if (diagnosticVisualizationParent != null)
            {
                if (Application.isEditor)
                {
                    Object.DestroyImmediate(diagnosticVisualizationParent);
                }
                else
                {
                    diagnosticVisualizationParent.transform.DetachChildren();
                    Object.Destroy(diagnosticVisualizationParent);
                }

                diagnosticVisualizationParent = null;
            }
        }

        #endregion IMixedRealityService

        #region IMixedRealityDiagnosticsSystem

        private MixedRealityDiagnosticsProfile diagnosticsSystemProfile = null;

        /// <inheritdoc/>
        public MixedRealityDiagnosticsProfile DiagnosticsSystemProfile
        {
            get
            {
                if (diagnosticsSystemProfile == null)
                {
                    diagnosticsSystemProfile = ConfigurationProfile as MixedRealityDiagnosticsProfile;
                }
                return diagnosticsSystemProfile;
            }
        }

        private bool showDiagnostics;

        private bool previousShowProfiler;

        /// <inheritdoc />
        public bool ShowDiagnostics
        {
            get { return showDiagnostics; }

            set
            {
                if (value != showDiagnostics)
                {
                    showDiagnostics = value;

                    // The voice commands are handled by the diagnosticVisualizationParent GameObject, we cannot disable the parent 
                    // or we lose the ability to re-show the visualizations. Instead, disable
                    if(ShowProfiler)
                    {
                        visualProfiler.IsVisible = value;
                    }
                }
            }
        }

        private bool showProfiler;

        /// <inheritdoc />
        public bool ShowProfiler
        {
            get
            {
                return showProfiler;
            }

            set
            {
                if (value != showProfiler)
                {
                    showProfiler = value;
                    if ((visualProfiler != null) && ShowDiagnostics)
                    {
                        visualProfiler.IsVisible = value;
                    }
                }
            }
        }

        private bool showFrameInfo;

        /// <inheritdoc />
        public bool ShowFrameInfo
        {
            get
            {
                return showFrameInfo;
            }

            set
            {
                if (value != showFrameInfo)
                {
                    showFrameInfo = value;
                    if (visualProfiler != null)
                    {
                        visualProfiler.FrameInfoVisible = value;
                    }
                }
            }
        }

        private bool showMemoryStats;

        /// <inheritdoc />
        public bool ShowMemoryStats
        {
            get
            {
                return showMemoryStats;
            }

            set
            {
                if (value != showMemoryStats)
                {
                    showMemoryStats = value;
                    if (visualProfiler != null)
                    {
                        visualProfiler.MemoryStatsVisible = value;
                    }
                }
            }
        }

        private float frameSampleRate = 0.1f;

        /// <inheritdoc />
        public float FrameSampleRate
        {
            get
            {
                return frameSampleRate;
            }

            set
            {
                if (!Mathf.Approximately(frameSampleRate, value))
                {
                    frameSampleRate = value;

                    if (visualProfiler != null)
                    {
                        visualProfiler.FrameSampleRate = frameSampleRate;
                    }
                }
            }
        }

        #endregion IMixedRealityDiagnosticsSystem

        #region IMixedRealityEventSource

        private DiagnosticsEventData eventData;

        /// <inheritdoc />
        public uint SourceId => (uint)SourceName.GetHashCode();

        /// <inheritdoc />
        public string SourceName => "Mixed Reality Diagnostics System";

        /// <inheritdoc />
        public new bool Equals(object x, object y) => false;

        /// <inheritdoc />
        public int GetHashCode(object obj) => SourceName.GetHashCode();

        private void RaiseDiagnosticsChanged()
        {
            eventData.Initialize(this);
            HandleEvent(eventData, OnDiagnosticsChanged);
        }

        /// <summary>
        /// Event sent whenever the diagnostics visualization changes.
        /// </summary>
        private static readonly ExecuteEvents.EventFunction<IMixedRealityDiagnosticsHandler> OnDiagnosticsChanged =
            delegate (IMixedRealityDiagnosticsHandler handler, BaseEventData eventData)
            {
                var diagnosticsEventsData = ExecuteEvents.ValidateEventData<DiagnosticsEventData>(eventData);
                handler.OnDiagnosticSettingsChanged(diagnosticsEventsData);
            };

        #endregion IMixedRealityEventSource

        private TextAnchor windowAnchor = TextAnchor.LowerCenter;

        /// <summary>
        /// What part of the view port to anchor the window to.
        /// </summary>
        public TextAnchor WindowAnchor
        {
            get { return windowAnchor; }

            set
            {
                if (value != windowAnchor)
                {
                    windowAnchor = value;

                    if (visualProfiler != null)
                    {
                        visualProfiler.WindowAnchor = windowAnchor;
                    }
                }
            }
        }

        private Vector2 windowOffset = new Vector2(0.1f, 0.1f);

        /// <summary>
        /// The offset from the view port center applied based on the window anchor selection.
        /// </summary>
        public Vector2 WindowOffset
        {
            get { return windowOffset; }

            set
            {
                if (value != windowOffset)
                {
                    windowOffset = value;

                    if (visualProfiler != null)
                    {
                        visualProfiler.WindowOffset = windowOffset;
                    }
                }
            }
        }

        private float windowScale = 1.0f;

        /// <summary>
        /// Use to scale the window size up or down, can simulate a zooming effect.
        /// </summary>
        public float WindowScale
        {
            get { return windowScale; }

            set
            {
                if (value != windowScale)
                {
                    windowScale = value;

                    if (visualProfiler != null)
                    {
                        visualProfiler.WindowScale = windowScale;
                    }
                }
            }
        }

        private float windowFollowSpeed = 5.0f;

        /// <summary>
        /// How quickly to interpolate the window towards its target position and rotation.
        /// </summary>
        public float WindowFollowSpeed
        {
            get { return windowFollowSpeed; }

            set
            {
                if (value != windowFollowSpeed)
                {
                    windowFollowSpeed = value;

                    if (visualProfiler != null)
                    {
                        visualProfiler.WindowFollowSpeed = windowFollowSpeed;
                    }
                }
            }
        }
    }
}