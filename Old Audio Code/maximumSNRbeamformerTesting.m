function [yOut, yNoiseOut, yCleanOut, RTrank, RIrank, RTcond, RIcond ] = maximumSNRbeamformerTesting( cleanSignal,noiseSignal,trueSignal,p,lambda )
%UNTITLED Summary of this function goes here
%   Detailed explanation goes here
%   trueSignal = y;
%   cleanSignal = x;
%   noiseSignal = v;
%P = last p frames used to estimnate noise and target out from;

fftLength = 1024;
fftJump = fftLength/4;
microphoneAmount = length(cleanSignal);
stft_ = stft(cleanSignal{1},fftLength,fftJump,1);
siz__ = size(stft_);
Pframe = p;
stft_2 = stft(trueSignal{1},fftLength,fftJump,1);
siz__2 = size(stft_2);
ySTFT = zeros(siz__2);
yNoiseSTFT = zeros(siz__2);
ycleanSTFT = zeros(siz__2);

if Pframe < microphoneAmount
    Pframe = microphoneAmount;
    
elseif Pframe == inf
    Pframe = siz__(2);
elseif Pframe > siz__(2)
    Pframe = siz__(2);
elseif Pframe == 'half'
    Pframe = round(siz__(2)/2);
end

%save('test2.mat', 'stft_')
microphoneCleanSTFTCell = cell(microphoneAmount,1);
microphoneNoiseSTFTCell = cell(microphoneAmount,1);
microphoneTrueSTFTCell = cell(microphoneAmount,1);
microphoneXTSTFTCell = cell(microphoneAmount,1);
microphoneXISTFTCell = cell(microphoneAmount,1);

for i = 1:microphoneAmount
    stft_ = stft(cleanSignal{i},fftLength,fftJump,1);
    siz_ = size(stft_);
    microphoneXTSTFTCell{i} = stft_(:,siz_(2)-Pframe+1:siz_(2));
    
    stft_ = stft(noiseSignal{i},fftLength,fftJump,1);
    microphoneXISTFTCell{i} = stft_(:,siz_(2)-Pframe+1:siz_(2));
end

%save('test1.mat', 'microphoneXTSTFTCell','microphoneXISTFTCell')

for i = 1:microphoneAmount
    stft_ = stft(trueSignal{i},fftLength,fftJump,1);
    microphoneTrueSTFTCell{i} = stft_;
    stft_ = stft(noiseSignal{i},fftLength,fftJump,1);
    microphoneNoiseSTFTCell{i} = stft_;
    stft_ = stft(cleanSignal{i},fftLength,fftJump,1);
    microphoneCleanSTFTCell{i} = stft_;
end

partsReal = siz__(2);

parts = Pframe;
frequencyAmount = siz__(1);

for ww = 1:frequencyAmount
    for t = 1:parts
        for w = 1:microphoneAmount %
            XTstft_ = microphoneXTSTFTCell{w};
            XIstft_ = microphoneXISTFTCell{w};
            xT(w,t) = XTstft_(ww,t); %these needs to be estimated as the actual sounds the gains work on are both of target and noise
            xI(w,t) = XIstft_(ww,t); %t is the frame, w is the microphone. Thus it creates a matrix of microphone size * frames
        end
    end
    %save('test3.mat', 'xT','xI')
    xTr_ = zeros(microphoneAmount);
    for t=1:parts
        xT_ = xT(:,t);
        xT_conj = xT_';
        xTr = xT_*xT_conj;
        xTr_ = xTr_ + xTr; %summing
    end
    
    xIr_ = zeros(microphoneAmount);
    for t=1:parts
        xI_ = xI(:,t);
        xI_conj = xI_';
        xIr = xI_*xI_conj;
        xIr_ = xIr_ + xIr; %summing
    end
    
    RT = (1/parts) * (xTr_);
    RI = (1/parts) * (xIr_);
    RT = (1-lambda)*RT + (lambda*(trace(RT)/length(RT)))*eye(size(RT)); %does not seem like that the paper fixes rank deficiency 
    RI = (1-lambda)*RI + (lambda*(trace(RI)/length(RI)))*eye(size(RI));
    RTrank(ww) = rank(RT);
    RIrank(ww) = rank(RI);
    RTcond(ww) = cond(RT);
    RIcond(ww) = cond(RI);
    
    [V,D] = eig(RI\RT);
    [~,i] = max(D(:));
    [~, I_col] = ind2sub(size(D),i);
    v = V(:,I_col);
    %test(ww) = i;
    %save('test7.mat', 'D','V','RT','RI','xTr_','xIr_','v','test')
    %v = v';
    
    %hmax = ((v*v')/max(D(:)));
    %save('test9.mat','hmax','v')
    
    for t = 1:partsReal
        for w = 1:microphoneAmount %
            Xstft_ = microphoneTrueSTFTCell{w};
            x(w,t) = Xstft_(ww,t); 
        end
    end
    xr_ = zeros(microphoneAmount);
    for t=1:partsReal
        x_ = x(:,t);
        x_conj = x_';
        xr = x_*x_conj;
        xr_ = xr_ + xr; %summing
    end
    RX = (1/partsReal) * (xr_);
    %save('test10.mat','v')
    b = (RX*v)/(v'*RX*v);
    %[bMax,bI] = max(b);
    %save('test9.mat','RX','b','v')
    
    for t = 1:partsReal
        for w = 1:microphoneAmount %
            
            Xstft_ = microphoneTrueSTFTCell{w};
            X(w,1) = Xstft_(ww,t);
            
            XNoisestft_ = microphoneNoiseSTFTCell{w};
            XNoise(w,1) = XNoisestft_(ww,t);
            
            XCleanstft_ = microphoneCleanSTFTCell{w};
            XClean(w,1) = XCleanstft_(ww,t);
        %Xr = X*X';
        %save('test10.mat','X','v')
        %hmax_ = hmax*(Xr*eye(length(Xr),1));
        %save('test10.mat','hmax_')
        end
        ySTFT(ww,t) = (b(1)*v)'*X;
        yNoiseSTFT(ww,t) = (b(1)*v)'*XNoise;
        ycleanSTFT(ww,t) = (b(1)*v)'*XClean;
    end
    %end
    
    
    
end

%save('test6.mat','X','ySTFT')
yOut = istft(ySTFT,fftLength,fftJump,1);
yNoiseOut = istft(yNoiseSTFT,fftLength,fftJump,1);
yCleanOut = istft(ycleanSTFT,fftLength,fftJump,1);
clearvars -except yOut yNoiseOut yCleanOut RTrank RIrank RTcond RIcond
end

%end

