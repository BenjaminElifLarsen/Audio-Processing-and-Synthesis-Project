function [ cost,W,H,Spectrogram ] = NMFSparsity( S,W,H,Lambda,Beta, NumberOfInterations, Spectra )
%UNTITLED2 Summary of this function goes here
%   Detailed explanation goes here
if isequal('magnitude',Spectra)
    V = abs((S));
elseif isequal('power',Spectra)
    V = abs((S)).^2;
elseif isequal('log',Spectra)
    
end

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

wn = sqrt(sum(W.^2));
W  = bsxfun(@rdivide,W,wn);
H  = bsxfun(@times,  H,wn');


%cost(1) = costFunc(S,A,X,CostFunction)
WHat = cell(1,NumberOfInterations);
HHat = cell(1,NumberOfInterations);
cost = zeros(1,NumberOfInterations);
%SHat = cell(1,NumberOfInterations);

for z = 1:NumberOfInterations
    NMFInterationNumber = z
    [cost(z),W,H] = iterations(V,W,H,Beta,Wk,Hk,Lambda);
    WHat{z} = W;
    HHat{z} = H;
end

[~,MinCost] = min(cost);
W = WHat{MinCost};
H = HHat{MinCost};
Spectrogram = W*H;
end


function [cost_,W,H] = iterations(V,W,H,Beta,Wk,Hk,Lambda)
[F,N] = size(V);
%Updates W
if Wk == 0
    %A = A.*(((Ymagnitude./(A*X)*X')./(ones(size(Ymagnitude))*X')));
    W = W.*((((W*H).^(Beta-2).*V)*H' + W.*(ones(length(W))*(W.*((W*H).^(Beta-1)*H')))).*(((W*H).^(Beta-1)*H' + W.*(ones(length(W))*(W.*(((W*H).^(Beta-2).*V)*H')))).^-1));
    %W = W.* ((V.* (W*H).^(Beta -2) )*H') ./( (W*H) .^(Beta -1) *H');
    if Wk == 0 
        %W = W .* repmat(scale.^-1,F,1);
        W = bsxfun(@rdivide,W,sqrt(sum(W.^2)));
    end
end
%Updates H
if Hk == 0
    H = H.*((W'*(V.*(W*H).^(Beta-2))).*((W'*(W*H).^(Beta-1)+Lambda).^-1));
    %H = H.*(W'*( V.*(W*H).^(Beta-2)))./(W'*(W*H).^(Beta-1) );
    %X = X.*((A'*(Ymagnitude./(A*X)))./(A'*ones(size(Ymagnitude))));
end

if Wk == 0 && Hk == 0
    scale = sqrt(sum(W.^2,1));
    if Wk == 0 
        %W = W .* repmat(scale.^-1,F,1);
        %W = bsxfun(@rdivide,W,sqrt(sum(W.^2)));
    end
    if Hk == 0
        %H = H .* repmat(scale',1,N);
    end
end

VHat = W*H;
cost_ = costFunc(V,VHat,Beta);
end

%cost function
function [cost] = costFunc(V,VHat, Beta)
siz = size(V);
if Beta == 1 %KL
    cost = (sum( V(:).*log(V(:)./VHat(:))+(VHat(:)-V(:)),'omitnan'));
elseif Beta == 0 %IS
    cost = (sum( V(:).*VHat(:).^-1 - log(V(:).*VHat(:).^-1),'omitnan'))-(siz(1)*siz(2));
end

end
