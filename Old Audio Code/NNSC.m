function [ cost,W,H,Spectrogram ] = NNSC(S,W,H,NumberOfInterations, lambda)
%UNTITLED Summary of this function goes here
%   Detailed explanation goes here

V = abs((S)).^2;


W_ = sum(abs(W(:)));

H_ = sum(abs(H(:)));

if W_ == 0
    %W =  0.1 + (221.1-0.1)*rand(size(W));
    W =  0.1 + (1.1-0.1)*rand(size(W));
    Wk = 0;
else
    Wk = 1;
end

if H_ == 0
    %H =  0.1 + (221.1-0.1)*rand(size(H));
    H =  0.1 + (1.1-0.1)*rand(size(H));
    Hk = 0;
else
    Hk = 1;
end

WHat = cell(1,NumberOfInterations);
HHat = cell(1,NumberOfInterations);
cost = zeros(1,NumberOfInterations);

for z = 1:NumberOfInterations
    NNSCInterationNumber = z
    [cost(z),W,H] = iterations(V,W,H,Wk,Hk,lambda);
    WHat{z} = W;
    HHat{z} = H;
end

[~,MinCost] = min(cost);
W = WHat{MinCost};
H = HHat{MinCost};
Spectrogram = W*H;

end

function [cost_,W,H] = iterations(V,W,H,Wk,Hk,lambda)
[F,N] = size(V);

if Wk == 0 && Hk == 0
    scale = sqrt(sum(W.^2,1));
    if Wk == 0
        W = W .* repmat(scale.^-1,F,1);
    end
    if Hk == 0
        H = H .* repmat(scale',1,N);
    end
    %elseif  Hk == 0
    %    scale = sqrt(sum(W.^2,1));
    %
    %    H = H .* repmat(scale',1,N);
end

if Wk == 0
    W = W.*((V*H'+W.*(ones(length(W))*(W*H*H'.*W))).*(W*H*H'+W.*(ones(length(W))*(V*H'.*W))).^-1);
end

if Hk == 0
    H = H.*((W'*V).*(W'*W*H+lambda).^-1);
end




VHat = W*H;
cost_ = costFunc(V,VHat);

end

function [cost] = costFunc(V,VHat)
%siz = size(V);
cost = sum((V(:)-VHat(:)).^2);

end

