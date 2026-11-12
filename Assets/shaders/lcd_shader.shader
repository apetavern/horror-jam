//=========================================================================================================================
// Optional
//=========================================================================================================================
HEADER
{
    Description = "LCD Screen Shader";
}

//=========================================================================================================================
// Optional
//=========================================================================================================================
FEATURES
{
    #include "common/features.hlsl"
}

//=========================================================================================================================
// Optional
//=========================================================================================================================
MODES
{
    Forward();
    Depth( S_MODE_DEPTH );
    ToolsShadingComplexity( "tools_shading_complexity.shader" );
}

//=========================================================================================================================

COMMON
{
    #include "common/shared.hlsl"
}

//=========================================================================================================================

struct VertexInput
{
    #include "common/vertexinput.hlsl"
};

//=========================================================================================================================

struct PixelInput
{
    #include "common/pixelinput.hlsl"
};

//=========================================================================================================================

VS
{
    #include "common/vertex.hlsl"
    PixelInput MainVs( VertexInput i )
    {
        PixelInput o = ProcessVertex( i );
        return FinalizeVertex( o );
    }
}

//=========================================================================================================================

PS
{
    #define CUSTOM_TEXTURE_FILTERING
    SamplerState TextureFiltering < Filter( POINT ); AddressU( WRAP ); AddressV( WRAP ); AddressW( WRAP ); MaxAniso( 8 ); >;

    #include "common/utils/Material.CommonInputs.hlsl"
    #include "common/pixel.hlsl"
    //=====================================================================================================================
    // Properties
    //=====================================================================================================================

    //
    // Appearance
    //
    float g_flBrightnessMultiplier< UiGroup( "LCD Screen/Appearance" ); UiType( Slider ); Default( 8.0f ); Range( 0.1f, 16.0f ); >;
    FloatAttribute( g_flBrightnessMultiplier, true );

    float2 g_vScreenResolution< UiGroup( "LCD Screen/Appearance" ); UiType( Slider ); Default2( 256.0f, 256.0f ); Range2( 0.0f, 0.0f, 1024.0f, 1024.0f ); >;

    //
    // Subpixels
    //
    CreateInputTexture2D( PixelTexture, Srgb, 8, "", "_color", "LCD Screen/Subpixels", Default3( 1.0, 1.0, 1.0 ) );
    Texture2D g_tPixel < Channel( RGB, Box( PixelTexture ), Srgb ); OutputFormat( BC7 ); SrgbRead( true ); >;

    float2 g_vPixelSize< UiGroup( "LCD Screen/Subpixels" ); UiType( Slider ); Default2( 1.0f, 1.0f ); >;

    float g_flPixelOpacityMin< UiGroup( "LCD Screen/Subpixels" ); UiType( Slider ); Default( 0.1f ); Range( 0.0f, 1.0f ); >;
    FloatAttribute( g_flPixelOpacityMin, true );

    float g_flPixelOpacityMax< UiGroup( "LCD Screen/Subpixels" ); UiType( Slider ); Default( 1.0f ); Range( 0.0f, 1.0f ); >;
    FloatAttribute( g_flPixelOpacityMax, true );

    float g_flPixelBlendOffset< UiGroup( "LCD Screen/Subpixels" ); UiType( Slider ); Default( 0.0f ); Range( -32, 32.0f ); >;
    FloatAttribute( g_flPixelBlendOffset, true );

    float g_flPixelBlendDistance< UiGroup( "LCD Screen/Subpixels" ); UiType( Slider ); Default( 8.0f ); Range( 4.0f, 1024.0f ); >;
    FloatAttribute( g_flPixelBlendDistance, true );

    //
    // Viewing Angles
    //
    float g_flFresnelReflectance< UiGroup( "LCD Screen/Viewing Angles" ); UiType( Slider ); Default( 0.01f ); Range( 0.0f, 1.0f ); >;
    FloatAttribute( g_flFresnelReflectance, true );

    float g_flFresnelExponent< UiGroup( "LCD Screen/Viewing Angles" ); UiType( Slider ); Default( 5.0f ); Range( 0.0f, 10.0f ); >;
    FloatAttribute( g_flFresnelExponent, true );

    //
    // animation
    //
    float2 g_vAnimationScroll< UiGroup( "LCD Screen/Animation" ); UiType( Slider ); Default2( 0.0f, 0.0f ); Range2( -1.0f, -1.0f, 1.0f, 1.0f ); >;

    //=====================================================================================================================

    float4 MainPs( PixelInput i ) : SV_Target0
    {
        float2 vSnappedUvs = round( i.vTextureCoords.xy * g_vScreenResolution.xy + 0.5 ) / g_vScreenResolution.xy;
        vSnappedUvs += g_vAnimationScroll * g_flTime;

        float2 vLcdUvs = i.vTextureCoords.xy * g_vScreenResolution.xy / g_vPixelSize.xy;

        float4 vAlbedoSample = g_tColor.Sample( TextureFiltering, vSnappedUvs );
        float4 vLcdSample = g_tPixel.Sample( TextureFiltering, vLcdUvs );
        // Make the individual subpixels match the albedo sample
        vLcdSample *= vAlbedoSample;

        //
        // Invert using fresnel
        //
        float4 inverse = 1.0 - vAlbedoSample;
        // Increase saturation
        inverse = pow( inverse, 4 );
        // Decrease strength
        inverse = lerp( vAlbedoSample, inverse, 0.25 );
        inverse.a = 1.0;

        float3 vPositionWs = i.vPositionWithOffsetWs.xyz + g_vHighPrecisionLightingOffsetWs.xyz;
        float3 vToCam = normalize( g_vCameraPositionWs - vPositionWs );
        float ndotv = saturate( dot( normalize( i.vNormalWs ), vToCam ) );
        float fresnel = g_flFresnelReflectance + ( 1.0 - g_flFresnelReflectance ) * pow( 1.0 - ndotv, g_flFresnelExponent );
        vAlbedoSample = lerp( vAlbedoSample, inverse, fresnel );
        
        //
        // LCD subpixel blending
        //
        float fBlendAmt = ddx( vLcdUvs.x ) + ddy( vLcdUvs.y );
        fBlendAmt *= g_flPixelBlendDistance;
        fBlendAmt = 1.0f - ( fBlendAmt - g_flPixelBlendOffset );
        fBlendAmt = clamp( fBlendAmt, g_flPixelOpacityMin, g_flPixelOpacityMax );

        //
        // Output
        //
        float4 o = lerp( vAlbedoSample, vLcdSample, fBlendAmt );
        o *= g_flBrightnessMultiplier;
        o.a = 1.0;

        return o;
    }
}