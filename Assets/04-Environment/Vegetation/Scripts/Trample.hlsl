// MIT License
//
// Copyright (c) 2020 NedMakesGames

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files(the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and / or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions :

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

// Make sure this file is not included twice
#ifndef GRASS_TRAMPLE_INCLUDED
#define GRASS_TRAMPLE_INCLUDED

// Trample global variables
float _NumGrassTramplePositions;
// The length should match the max value in the renderer feature
float4 _GrassTramplePositions[8];

void CalculateTrample_float(float3 WorldPosition, float MaxDistance, float Falloff, float PushAwayStrength, float PushDownStrength,
    out float3 Offset, out float WindMultiplier) {
    Offset = 0;
    WindMultiplier = 1;
#ifndef SHADERGRAPH_PREVIEW
    // For each trample position
    for (int i = 0; i < _NumGrassTramplePositions; i++) {
        // Find the distance to the trample position
        float3 objectPositionWS = _GrassTramplePositions[i].xyz;
        float3 distanceVector = WorldPosition - objectPositionWS;
        float distance = length(distanceVector);
		
        // Calculate the trample strength. Assumes fall off is > 0
        // Strength will be zero if distance > max range
        float strength = 1 - pow(saturate(distance / MaxDistance), Falloff);
		
        // We want to apply a push away offset in the XZ plane. So normalize the
        // distance vector in this plane and then apply the push away strength
        float3 xzDistance = distanceVector;
        xzDistance.y = 0;
        float3 pushAwayOffset = normalize(xzDistance) * PushAwayStrength * strength;
		
        // The squish offset should point downwards
        float3 squishOffset = float3(0, -1, 0) * PushDownStrength * strength;
		
        // Add both offsets to the total offset
        Offset += pushAwayOffset + squishOffset;

        // Calculate a wind multiplier to suppress wind when this grass is being trampled
        WindMultiplier = min(WindMultiplier, 1 - strength);
    }
#endif
}

#endif