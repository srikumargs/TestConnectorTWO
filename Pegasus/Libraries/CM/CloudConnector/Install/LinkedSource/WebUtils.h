#pragma once

#include "..\LinkedSource\StringUtils.h"

void DeleteWebApp(std::ofstream_t& oLogStream, LPCWSTR lpcwstrAppPath);
void DeleteVirtualDir(std::ofstream_t& oLogStream, LPCWSTR lpcwstrAppPath);
void DeleteAppPool(std::ofstream_t& oLogStream, LPCWSTR lpcwstrAppPool);
void StopAppPool(std::ofstream_t& oLogStream, LPCWSTR lpcwstrAppPool);
void StartAppPool(std::ofstream_t& oLogStream, LPCWSTR lpcwstrAppPool);