﻿using BlazorAppHuellero.CustomStyle;
using Microsoft.AspNetCore.Components;

namespace BlazorAppHuellero.Layout
{
    public partial class LoginLayout : LayoutComponentBase
    {
        [Parameter]
        public RenderFragment Image { get; set; }

        [Inject]
        private AtowerTheme AtowerTheme { get; set; }
        private bool _isDarkMode { get; set; }

        protected override void OnInitialized()
        {
            _isDarkMode = false;
        }
    }
}
