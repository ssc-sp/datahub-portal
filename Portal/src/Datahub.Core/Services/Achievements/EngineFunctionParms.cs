﻿using System.Collections.Generic;

namespace Datahub.Core.Services.Achievements;

public record EngineFunctionParms(string CurrentMetric, HashSet<string> CurrentAchivements);
