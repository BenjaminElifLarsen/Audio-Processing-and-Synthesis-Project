function [ cost,W,H,Spectrogram ] = NMF( S,W,H,NumberOfAtoms,Beta, NumberOfInterations, Spectra )
%NMF (Non-negative matrix factorisation)
%   [ cost,W,H,Spectrogram ] = NMF( S,W,H,NumberOfAtoms,Beta, NumberOfInterations, Spectra )
%   This code implements a non-negative matrix factorisation. The
%   implemented cost-functions are Itakura–Saito and Kullback–Leibler. The calculations of the
%   activations and atoms are done using multiplicative update rules.
%   Inputs
%       S = spectrogram
%       W = Vector/Matrix of atoms of size
%       H = vector/Matrix of activations of size
%       NumberOfAtoms = the amount of atoms that should be used
%       Beta = the selection of cost function
%           If Beta = 1', cost function is Kullback–Leibler
%           divergence
%           If Beta = 0, cost function is Itakura–Saito
%           divergence
%       NumberOfInterations = the amount of times the calculations should
%       be run
%       If either A or X are zero vectors/matrices these will be calculated, else
%       they will be used. The number of atoms will be ignored
%       If Both A and X are zero, the amount of atoms are deterimed the size of
%       both
%   Outputs
%       cost = the cost function
%       W, H = returns the (pre-)calculated W and H vectors/matrices that
%       minimizes the cost function
%       Spectrogram = the return of the spectrogram selected by minimising
%       the cost function %maybe change name to stft
%   Spectra determines which kind of spectrogram that is used
%       'mangnitude', 'power', 'log'

%Maybe not use NumberOfAtoms
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

%cost(1) = costFunc(S,A,X,CostFunction)
WHat = cell(1,NumberOfInterations);
HHat = cell(1,NumberOfInterations);
cost = zeros(1,NumberOfInterations);
%SHat = cell(1,NumberOfInterations);

for z = 1:NumberOfInterations
    NMFInterationNumber = z
    [cost(z),W,H] = iterations(V,W,H,Beta,Wk,Hk);
    WHat{z} = W;
    HHat{z} = H;
end

[~,MinCost] = min(cost);
W = WHat{MinCost};
H = HHat{MinCost};
Spectrogram = W*H;
end


function [cost_,W,H] = iterations(V,W,H,Beta,Wk,Hk)
[F,N] = size(V);
%Updates W
if Wk == 0
    %A = A.*(((Ymagnitude./(A*X)*X')./(ones(size(Ymagnitude))*X')));
    W = W.*((((W*H).^(Beta-2).*V)*H').*(((W*H).^(Beta-1)*H').^-1));
    %W = W.* ((V.* (W*H).^(Beta -2) )*H') ./( (W*H) .^(Beta -1) *H');
end
%Updates H
if Hk == 0
    H = H.*((W'*((W*H).^(Beta-2).*V)).*((W'*(W*H).^(Beta-1)).^-1));
    %H = H.*(W'*( V.*(W*H).^(Beta-2)))./(W'*(W*H).^(Beta-1) );
    %X = X.*((A'*(Ymagnitude./(A*X)))./(A'*ones(size(Ymagnitude))));
end

if Wk == 0 && Hk == 0
    scale = sqrt(sum(W.^2,1));
    if Wk == 0 
        W = W .* repmat(scale.^-1,F,1);
    end
    if Hk == 0
        H = H .* repmat(scale',1,N);
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

