#pragma once

#include "..\LinkedSource\StringUtils.h"

int CleanupFirewallApplicationException(std::ofstream_t& oLogStream, const std::string_t& sImagePath);
int CleanupAdvancedFirewallRule(std::ofstream_t& oLogStream, const std::string_t& sRuleName);
