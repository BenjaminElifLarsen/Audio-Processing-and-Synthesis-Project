function [ SNRdB ] = ownSNR( cleanSignalPostPrecessing,noiseSignalPostProcessing )
%UNTITLED2 Summary of this function goes here
%   Detailed explanation goes here
    %SNRin = var(cleanSignalPreProcessing)/var(noiseSignalPreProcessing);
    SNRout = var(cleanSignalPostPrecessing)/var(noiseSignalPostProcessing);
    %SNRdelta = SNRout/SNRin;
    SNRdB = 10*log10(SNRout);
end

