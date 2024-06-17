// RGB conversion
//==============================================================================================================

public static void RGBToHSV(byte R, byte G, byte B, out double H, out double S, out double V)
{
    byte max = R > G ? R > B ? R : B : G > B ? G : B;
    byte min = R < G ? R < B ? R : B : G < B ? G : B;

    H = 0.0; S = 0.0; V = max / 255.0; if (max != min)
    {
        if      (R == max) H = (0 + (double)(G - B) / (max - min)) * 60.0;
        else if (G == max) H = (2 + (double)(B - R) / (max - min)) * 60.0;
        else if (B == max) H = (4 + (double)(R - G) / (max - min)) * 60.0;

        if (H < 0.0) H += 360.0; S = 1.0 - ((double)min / max);
    }
}

public static void RGBToHSL(byte R, byte G, byte B, out double H, out double S, out double L)
{
    byte max = R > G ? R > B ? R : B : G > B ? G : B;
    byte min = R < G ? R < B ? R : B : G < B ? G : B;

    H = 0.0; S = 0.0; L = (max + min) / 510.0; if (max != min)
    {
        if      (R == max) H = (0 + (double)(G - B) / (max - min)) * 60.0;
        else if (G == max) H = (2 + (double)(B - R) / (max - min)) * 60.0;
        else if (B == max) H = (4 + (double)(R - G) / (max - min)) * 60.0;

        if (H < 0.0) H += 360.0; S = L <= 0.5 ?
            (double)(max - min) / (max + min) : (max - min) / (510.0 - max - min);
    }
}

public static void RGBToHSI(byte R, byte G, byte B, out double H, out double S, out double I)
{
    byte max = R > G ? R > B ? R : B : G > B ? G : B;
    byte min = R < G ? R < B ? R : B : G < B ? G : B;

    H = 0.0; S = 0.0; I = (R + G + B) / 765.0; if (max != min)
    {
        if      (R == max) H = (0 + (double)(G - B) / (max - min)) * 60.0;
        else if (G == max) H = (2 + (double)(B - R) / (max - min)) * 60.0;
        else if (B == max) H = (4 + (double)(R - G) / (max - min)) * 60.0;

        if (H < 0.0) H += 360.0; S = 1 - ((min / 255.0) / I);
    }
}

public static void RGBToCMYK(byte R, byte G, byte B, out double C, out double M, out double Y, out double K)
{
    byte max = R > G ? R > B ? R : B : G > B ? G : B;

    K = 1.0 - (max / 255.0); //if (K < 0 || double.IsNaN(K)) K = 0;

    C = 0.0; M = 0.0; Y = 0.0; if (max > 0)
    {
        C = 1.0 - ((double)R / max); //if (C < 0 || double.IsNaN(C)) C = 0;
        M = 1.0 - ((double)G / max); //if (M < 0 || double.IsNaN(M)) M = 0;
        Y = 1.0 - ((double)B / max); //if (Y < 0 || double.IsNaN(Y)) Y = 0;
    }
}

// HSV conversion
//==============================================================================================================

public static void HSVToRGB(double H, double S, double V, out byte R, out byte G, out byte B)
{
    H /= 60.0; int i = (int)H;                  // H'
    double m = V * (1 - S);                     // min
    double T = V * (1 - (1 - (H - i)) * S);     // X + min \ even
    double Q = V * (1 - (H - i) * S);           // X + min \ odd

    /*double round = 0.00005;*/ switch (i) { default:
        case 0: R = (byte)((V + 0.00005) * 255); G = (byte)((T + 0.00005) * 255); B = (byte)((m + 0.00005) * 255); break;
        case 1: R = (byte)((Q + 0.00005) * 255); G = (byte)((V + 0.00005) * 255); B = (byte)((m + 0.00005) * 255); break;
        case 2: R = (byte)((m + 0.00005) * 255); G = (byte)((V + 0.00005) * 255); B = (byte)((T + 0.00005) * 255); break;
        case 3: R = (byte)((m + 0.00005) * 255); G = (byte)((Q + 0.00005) * 255); B = (byte)((V + 0.00005) * 255); break;
        case 4: R = (byte)((T + 0.00005) * 255); G = (byte)((m + 0.00005) * 255); B = (byte)((V + 0.00005) * 255); break;
        case 5: R = (byte)((V + 0.00005) * 255); G = (byte)((m + 0.00005) * 255); B = (byte)((Q + 0.00005) * 255); break;
    }
}

public static void HSVToHSL(double S, double V, out double SL, out double L)
{
    L = V * (1 - 0.5 * S); // L = V * (1 - (S / 2));
    SL = (L > 0 && L < 1) ? (V - L) / (L < .5 ? L : 1 - L) : 0.0;
}

public static void HSVToHSI(double H, double S, double V, out double SI, out double I)
{
    H /= 60.0; int i = (int)H;
    double Z = (i & 1) > 0 ? 1 + H - i : 2 + i - H;
    SI = S * (Z - 3) / (S * Z - 3); I = V * (3 - S * Z) / 3;

    //-----------------------------------------------------------------------------------

    //double m = V * (1 - S);                         // min; V is max
    //double Z = (i & 1) > 0 ? H - i : 1 - (H - i);   // abs(1 - (H % 2))
    //double X = V * (1 - Z * S);                     // unknown value
    //I = (m + V + X) / 3.0;                          // (R + G + B) / 3
    //SI = I > 0 ? 1 - (m / I) : 0;                   // 1 - (min / I)
}

public static void HSVToCMYK(double H, double S, double V, out double C, out double M, out double Y, out double K)
{
    H /= 60.0; int i = (int)H;
    
    K = 1.0 - V; switch (i) { default:
        case 0: C = 0; M = S * (1 - H + i); Y = S; break;
        case 1: C = S * (H - i); M = 0; Y = S; break;
        case 2: C = S; M = 0; Y = S * (1 - H + i); break;
        case 3: C = S; M = S * (H - i); Y = 0; break;
        case 4: C = S * (1 - H + i); M = S; Y = 0; break;
        case 5: C = 0; M = S; Y = S * (H - i); break;
    }
}

// HSL conversion
//==============================================================================================================

public static void HSLToRGB(double H, double S, double L, out byte R, out byte G, out byte B)
{
    H /= 60.0; int i = (int)H;                      // H'
    double C = (L < .5 ? 2 * L : 2 - (2 * L)) * S;  // max - min
    double m = L - (C * 0.5);                       // min
    double T = C * (H - i);                         // X \ even
    double Q = C * (1 - (H - i));                   // X \ odd

    /*double round = 0.00005;*/ switch (i) { default:
        case 0: R = (byte)((C + m + 0.00005) * 255); G = (byte)((T + m + 0.00005) * 255); B = (byte)((0 + m + 0.00005) * 255); break;
        case 1: R = (byte)((Q + m + 0.00005) * 255); G = (byte)((C + m + 0.00005) * 255); B = (byte)((0 + m + 0.00005) * 255); break;
        case 2: R = (byte)((0 + m + 0.00005) * 255); G = (byte)((C + m + 0.00005) * 255); B = (byte)((T + m + 0.00005) * 255); break;
        case 3: R = (byte)((0 + m + 0.00005) * 255); G = (byte)((Q + m + 0.00005) * 255); B = (byte)((C + m + 0.00005) * 255); break;
        case 4: R = (byte)((T + m + 0.00005) * 255); G = (byte)((0 + m + 0.00005) * 255); B = (byte)((C + m + 0.00005) * 255); break;
        case 5: R = (byte)((C + m + 0.00005) * 255); G = (byte)((0 + m + 0.00005) * 255); B = (byte)((Q + m + 0.00005) * 255); break;
    }
}

public static void HSLToHSV(double S, double L, out double SV, out double V)
{
    V = L + S * (L < .5 ? L : 1 - L);
    SV = V == 0 ? 0 : 2 * (1 - (L / V));
}

public static void HSLToHSI(double H, double S, double L, out double SI, out double I)
{
    H /= 60.0; int i = (int)H;
    double Z = (i & 1) == 0 ? H - i : 1 - (H - i);
    double C = (L < .5 ? 2 * L : 2 - (2 * L)) * S;
    I = L + C * (2 * Z - 1) / 6; SI = I > 0 ? 1 - ((L - (C * 0.5)) / I) : 0;

    //-----------------------------------------------------------------------------------

    //double Z = (i & 1) == 0 ? H - i : 1 - (H - i);  // abs(1 - (H % 2))
    //double C = (L < .5 ? 2 * L : 2 - (2 * L)) * S;  // max - min
    //double min = L - (C * 0.5);                     // min
    //double max = C + min;                           // max
    //double X = (C * Z) + min;                       // unknown value
    //I = (min + max + X) / 3.0;                      // (R + G + B) / 3
    //SI = I > 0 ? 1 - (min / I) : 0;                 // 1 - (min / I)
}

public static void HSLToCMYK(double H, double S, double L, out double C, out double M, out double Y, out double K)
{
    H /= 60.0; int i = (int)H;                      // H'
    double T = (L < .5 ? 2 * L : 2 - (2 * L)) * S;  // max - min

    K = 1 - L - (T * 0.5); switch (i) { default:
        case 0: C = 0; M = (2 * T * (1 - (H - i))) / (2 * L + T); Y = T / (L + (T * 0.5)); break;
        case 1: C = (2 * T * (H - i)) / (2 * L + T); M = 0; Y = T / (L + (T * 0.5)); break;
        case 2: C = T / (L + (T * 0.5)); M = 0; Y = (2 * T * (1 - (H - i))) / (2 * L + T); break;
        case 3: C = T / (L + (T * 0.5)); M = (2 * T * (H - i)) / (2 * L + T); Y = 0; break;
        case 4: C = (2 * T * (1 - (H - i))) / (2 * L + T); M = T / (L + (T * 0.5)); Y = 0; break;
        case 5: C = 0; M = T / (L + (T * 0.5)); Y = (2 * T * (H - i)) / (2 * L + T); break;
    }

    //-----------------------------------------------------------------------------------

    // Z = (i & 1) == 0 ? H - i : 1 - (H - i);  // abs(1 - (H % 2))
    // T = (L < .5 ? 2 * L : 2 - (2 * L)) * S;  // max - min
    // L - (T * 0.5)                            // min
    // T + (L - (T * 0.5))                      // max
    // (T * Z) + L - (T * 0.5)                  // X
    // 1 - (max / max)                          // 0
    // 1 - (min / max)                          // T / (L + (T * 0.5))
    // 1 - (X / max)                            // (2 * T * (1 - Z)) / (2 * L + T)
}

// HSI conversion
//==============================================================================================================

// CMYK conversion
//==============================================================================================================
