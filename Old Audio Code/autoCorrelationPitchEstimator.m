function [pitchEstimate, freqVector, costFunction] = ...
        autoCorrelationPitchEstimator(data, pitchBounds)
    delayBounds = [ceil(1/pitchBounds(2)), floor(1/pitchBounds(1))];
    delayVector = delayBounds(1):delayBounds(2);
    save('try.mat','delayBounds')
    nDelays = length(delayVector);
    % compute the cost function
    costFunction = nan(nDelays,1);
    for ii = 1:nDelays
        iDelay = delayVector(ii);
        truncatedData = data(1:end-iDelay);
        shiftedData = data(iDelay+1:end); % the +1 is to compensate for MATLAB's indexing
        costFunction(ii) = mean(truncatedData.*shiftedData);
    end
      
    % estimate the pitch
    [~,idx] = max(costFunction);
    
    pitchEstimate = 1/delayVector(idx);
    % return a freq vector and the cost function for these frequencies
    freqVector = flipud(1./delayVector(:));
    costFunction = flipud(costFunction);
end

