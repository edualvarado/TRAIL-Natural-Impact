using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MxMEditor
{

    public interface IPreviewable
    {
        void BeginPreview();
        void EndPreview();
        void UpdatePreview();
        void EndPreviewLocal();
    }
}