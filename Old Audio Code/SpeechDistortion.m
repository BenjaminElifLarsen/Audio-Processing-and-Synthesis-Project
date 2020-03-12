function [ SDR_,SDR2_ ] = SpeechDistortion( unprocessedSignalTarget, processedSignal )
%UNTITLED4 Summary of this function goes here
%   Detailed explanation goes here
SDR_ = 10*log10(var(unprocessedSignalTarget)/var(processedSignal)); %maybe this is only with the speech and not the interference signals 
SDR2_ = (var(unprocessedSignalTarget)/var(processedSignal));
%SDR2_ = ((rms(unprocessedSignalTarget)^2)/(rms(processedSignal)^2));
%%ask if you should have dB or not, since the paper have it in decible and
%%the paper they reference to 
end

