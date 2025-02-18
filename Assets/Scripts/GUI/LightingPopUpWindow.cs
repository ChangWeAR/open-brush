﻿// Copyright 2020 The Tilt Brush Authors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

// TODO: Better way to detect Passthrough support.
// Extra: Passthrough should be *per* envrionment really!
// See https://github.com/icosa-foundation/open-brush/issues/456
#if OCULUS_SUPPORTED
#define PASSTHROUGH_SUPPORTED
#endif

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace TiltBrush
{

    public class LightingPopUpWindow : PagingPopUpWindow
    {
        private string m_CurrentPresetGuid;
        private List<TiltBrush.Environment> m_Environments;

        protected override int m_DataCount
        {
            get { return m_Environments.Count; }
        }

        protected override void InitIcon(ImageIcon icon)
        {
            icon.m_Valid = true;
        }

        protected override void RefreshIcon(PagingPopUpWindow.ImageIcon icon, int iCatalog)
        {
            LightingButton iconButton = icon.m_IconScript as LightingButton;
            iconButton.SetPreset(m_Environments[iCatalog]);
            iconButton.SetButtonSelected(m_CurrentPresetGuid == m_Environments[iCatalog].m_Guid.ToString());
        }

        override public void Init(GameObject rParent, string sText)
        {
            //build list of lighting presets we're going to show
            m_Environments = EnvironmentCatalog.m_Instance.AllEnvironments.ToList();

            // Remove passthrough scene for devices that don't support it
#if !PASSTHROUGH_SUPPORTED
            foreach (var env in m_Environments)
            {
                // Passthrough
                if (env.m_Guid.ToString() == "e38af599-4575-46ff-a040-459703dbcd36")
                {
                    m_Environments.Remove(env);
                    break;
                }
            }
#endif // PASSTHROUGH_SUPPORTED

            //find the active lighting preset
            TiltBrush.Environment rCurrentPreset = SceneSettings.m_Instance.GetDesiredPreset();
            if (rCurrentPreset != null)
            {
                //find the index of our current preset in the preset list
                int iPresetIndex = -1;
                m_CurrentPresetGuid = rCurrentPreset.m_Guid.ToString();
                for (int i = 0; i < m_Environments.Count; ++i)
                {
                    if (m_Environments[i].m_Guid.ToString() == m_CurrentPresetGuid)
                    {
                        iPresetIndex = i;
                        break;
                    }
                }

                if (iPresetIndex != -1)
                {
                    //set our current page to show the active preset if we have more than one page
                    if (m_Environments.Count > m_IconCountFullPage)
                    {
                        m_RequestedPageIndex = iPresetIndex / m_IconCountNavPage;
                    }
                }
            }
            SceneSettings.m_Instance.FadingToDesiredEnvironment += OnFadingToDesiredEnvironment;

            base.Init(rParent, sText);
        }

        protected void OnFadingToDesiredEnvironment()
        {
            TiltBrush.Environment rCurrentPreset = SceneSettings.m_Instance.GetDesiredPreset();
            if (rCurrentPreset != null)
            {
                m_CurrentPresetGuid = rCurrentPreset.m_Guid.ToString();
            }
            RefreshPage();
        }

        void OnDestroy()
        {
            SceneSettings.m_Instance.FadingToDesiredEnvironment -= OnFadingToDesiredEnvironment;
        }
    }
} // namespace TiltBrush
