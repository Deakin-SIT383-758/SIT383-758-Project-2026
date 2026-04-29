#pragma once

extern "C"
{
    __declspec(dllexport) void prepareModel(char *dirname);
    __declspec(dllexport) int doRecognise(char *imageData, int width, int height);

    __declspec(dllexport) void retrieveMatch(int i, int &category, float &confidence, float &sx, float &sy, float &ex, float &ey);
}