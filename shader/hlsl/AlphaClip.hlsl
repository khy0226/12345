void AlphaClip_float(float albedoAlpha, float color, float cutoff, out float alpha)
{
#if SHADERGRAPH_PREVIEW
    alpha = 0;
#else

    alpha = albedoAlpha * color;
    clip(alpha - cutoff);
    

#endif
}

