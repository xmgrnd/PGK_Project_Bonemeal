Shader "Custom/URP_PortalMask"
{
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry-1" }
        
        ColorMask 0 // Nie rysuj kolorów
        ZWrite Off  // Nie zapisuj do głębi

        Pass
        {
            Stencil
            {
                Ref 1
                Comp Always
                Pass Replace
            }
        }
    }
}