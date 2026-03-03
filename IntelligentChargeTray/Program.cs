using IntelligentChargeTray;
using IntelligentChargeTray.Services;

Application.EnableVisualStyles();
Application.SetCompatibleTextRenderingDefault(false);
Application.Run(new TrayApplicationContext(new ChargeThresholdService(), new AutostartService()));
