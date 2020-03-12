function [ pitchEstimation,cost,frequencyVector ] = PitchEstimation(datasegment,lowerlimit, upperlimit, fs,method)
%AUTOCORRPIT Summary of this function goes here
%uppter and lower in fundamental frequency
%   Detailed explanation goes here
%

%

[TauBoundry] = TauB(lowerlimit, upperlimit, fs);

%call timeshift from the autocorr
%[x] = timeshift(datasegment, Tau);

[pitchEstimation,cost,frequencyVector] = autoCorr(datasegment,TauBoundry,method);
%pitchEstimation = pitchEstimation*fs;
end

function [Taub]= TauB(lower, upper, fs)
fb = [lower upper]/fs;
Taub = [ceil(1/fb(2)) floor(1/fb(1))];


end

function [pitchEstimation,vector,freqvector] = autoCorr(signal,TauBoundry,method)
upper = TauBoundry(2);
lower = TauBoundry(1);

delayVector = lower:upper;
delayAmount = length(delayVector);
cost = nan(delayAmount,1);

if strcmp(method, 'autoCorr')
    for t = 1:delayAmount
        TauDelay = delayVector(t);
        truncatedSignal = signal(1:end-TauDelay);
        shiftedSignal = signal(TauDelay+1:end);
        cost(t) = mean(truncatedSignal.*shiftedSignal);
        
    end
    [~,index] = max(cost);
    pitchEstimation = 1/delayVector(index); %fs/index for f0
    cost = flipud(cost);
    freqvector = nan(upper+lower,1);
    frequencyVector = flipud(1./delayVector(:));
    freqvector(lower:upper) = frequencyVector;
    size(cost);
    vector = nan(upper+lower,1);
    size(vector);
    vector(lower:upper) = cost;
    
elseif strcmp(method, 'combFilter')
    for t = 1:delayAmount
        TauDelay = delayVector(t);
        truncatedSignal = signal(1:end-TauDelay);
        shiftedSignal = signal(TauDelay+1:end);
        cost(t) = mean(abs(truncatedSignal-shiftedSignal).^2); %abs

    end
    [~,index] = min(cost);
    pitchEstimation = 1/delayVector(index); %fs/index for f0
    cost = flipud(cost);
    freqvector = nan(upper+lower,1);
    frequencyVector = flipud(1./delayVector(:));
    freqvector(lower:upper) = frequencyVector;
    size(cost);
    vector = nan(upper+lower,1);
    size(vector);
    vector(lower:upper) = cost;    
    
end

end


