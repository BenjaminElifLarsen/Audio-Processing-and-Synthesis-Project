function [ SignalAfter ] = dBchanger(SignalTarget, SignalNotTarget, Wanted_dB )
%UNTITLED2 Summary of this function goes here
%   Detailed explanation goes here



sigmai2Before = var(SignalNotTarget);

%signalChanger = dot(SignalTarget, SignalTarget)/10^(Wanted_dB/10);
signalChanger = var(SignalTarget)/10^(Wanted_dB/10);

SignalAfter = (SignalNotTarget*sqrt(signalChanger))/sqrt(sigmai2Before);

end

