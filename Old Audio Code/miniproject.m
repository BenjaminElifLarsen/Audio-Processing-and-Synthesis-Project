clear
clc
fs = 44100/2;

signalLength = 8; %length in seconds
output = zeros(1,fs*signalLength);
Nn = 16;
Tracker = 0;

bubbleLike = 3;
resonantLike = 4;
rigidLike = 5;


for ttt = 1:fs*signalLength % up to file length
    
    if Tracker  <= fs/1.2 && ttt <= fs*signalLength-(fs)
        Tracker;
        if(randi(3000) > 2999)
            Tracker;
            if randi(10) > bubbleLike %bubble
                alpha = 1.5 + (1.5-1.7).*rand(1);
                epsilon = 0.01 + (0.1-0.01).*rand(1);
                beta = 1;
                
                r = 0.002 + (0.007-0.002).*rand(1);
                D = 0.5 + (0.5-1).*rand(1)^beta;
                a = D*r^alpha;
                
                f0 = 3/r;
                d = 0.043*f0 + 0.0014*f0^(3/2);
                test = 1;
                index = 0;
                while test > 0.01
                    test = exp(-d*(index/fs));
                    index = index + 1;
                end
                
                sigma = epsilon * d;
                t = 0:1/fs:index/fs;
                ft = f0*(1+sigma.*t);
                ybubble = a*sin(2*pi.*ft.*t).*exp(-d.*t);
                output(ttt:ttt+index) = output(ttt:ttt+index) + (ybubble*240);
            end
            
            if randi(10) > resonantLike %resonant surface
                f(1) = 3547;
                a(1) = 0.4004162;
                f(2) = 4236;
                a(2) = 0.400558;
                f(3) = 198;
                a(3) = 0.43004344;
                f(4) = 1317;
                a(4) = 0.200474;
                f(5) = 4257;
                a(5) = 0.3003634;
                f(6) = 4487;
                a(6) = 0.7004734;
                f(7) = 4617;
                a(7) = 0.6003293;
                f(8) = 3708;
                a(8) = 0.3002785;
                f(9) = 5753;
                a(9) = 0.5004182;
                n = length(f);
                alpha_(1) = 75.035;
                alpha_(2) = 143.025;
                alpha_(3) = 85.025;
                alpha_(4) = 112.003;
                alpha_(5) = 81.0015;
                alpha_(6) = 455.003;
                alpha_(7) = 134.051;
                alpha_(8) = 235.015;
                alpha_(9) = 335.006;
                test = 1;
                index = 0;
                while test > 0.01
                    test = exp(-min(alpha_)*(index/fs));
                    index = index + 1;
                end
                t = 0:1/fs:index/fs;
                %for t = 1:index
                    
                    for m = 1:n
                        
                        x2_m = a(m)*sin(2*pi*f(m).*t).*exp(-alpha_(m).*t);
                        x2_(m,1:length(t)) = x2_m;
                    end
                    x2 = sum(x2_);
                    %x2(t) = sum(x2_);
                %end
                
                output(ttt:ttt+index) = output(ttt:ttt+index) + (x2*0.07);
            end
            
            if randi(10) > rigidLike  %rigid or deformable objects
                Tracker;
                
                subbands = (10000-100)/8;
                for i = 1:8
                    ESBS3(i) = 0.108*(subbands*i)+24.7;
                    alphaESB(i) = (56 + (160-56)*rand(1));
                end
                
                %for i = 1:8
                    index = 0;
                    test = 1;
                    while test > 0.01 
                        test = exp(-min(alphaESB)*(index/fs));
                        index = index + 1;
                    end
                %end                
                t = 0:1/fs:index/fs;
                noise = rand(1,length(t));
                for i = 1:8
                    halfbandwidth = ESBS3(i)/2;
                    [freqB,freqA] = butter(3,[i*subbands-halfbandwidth i*subbands+halfbandwidth]/(fs));
                    s{i} = filter(freqB,freqA, noise);
                    %alpha(i) = (0.0010 + (0.0244-0.0010).*rand(1))/fs;
                    ar(i) = 0.2 + (0.9-0.2).*rand(1);
                end
%                 for i = 1:8
%                     index(i) = 0;
%                     test = 1;
%                     while test > 0.01 && index(i) < fs
%                         test = exp(-alpha(i)*(index(i)));
%                         index(i) = index(i) + 1;
%                     end
%                 end
                ysur = zeros(1,max(index));
%                 for tt = 1:max(index)
%                     for i = 1:8
%                         s_ = s{i};
%                         x_(i,:) = a(i)*s_(tt)*exp(-alpha(i)*tt);
%                     end
%                     y_ = sum(x_);
%                     ysur(tt) = ysur(tt) + y_;
%                     ysur = ysur(1:max(index));
%                 end
                ysur = zeros(1,length(t));
                for i = 1:length(s)
                    ysur = ysur + (ar(i)*s{i}.*exp(-alphaESB(i).*t));
                end
                output(ttt:ttt+max(index)) = output(ttt:ttt+max(index)) + ysur*0.8;
            end
            
            
            Tracker = fs;
        end
    end
    
    Tracker = Tracker-1;
end


[y,fsrain] = audioread('rain.mp3');
if fsrain ~= fs
    [Pr,Qr] = rat(fs/fsrain);
    y = resample(y,Pr,Qr);
    %fs = 16000;
end
y3 = y(:,1);
subbands2 = (10000-100)/32;
for i = 1:32 %analysis
    ESBS4(i) = 0.108*(subbands2*i)+24.7;
end
for i = 1:32 %analysis
    halfbandwidth = ESBS4(i)/2;
    [freqB2,freqA2] = butter(3,[i*subbands2-halfbandwidth i*subbands2+halfbandwidth]/(fs/2));
    s2{i} = filter(freqB2,freqA2, y3);
    a2(i) = rms(s2{i});
end


y2 = zeros(1,fs*signalLength-(fs/2));
subbands2 = (10000-100)/32;
for i = 1:32 %rain background sound
    ESBS4(i) = 0.108*(subbands2*i)+24.7;
end

noise2 = rand(1,fs*signalLength-(fs/2));
for i = 1:32 %rain background sound
    halfbandwidth = ESBS4(i)/2;
    [freqB2,freqA2] = butter(3,[i*subbands2-halfbandwidth i*subbands2+halfbandwidth]/(fs/2));
    s3{i} = filter(freqB2,freqA2, noise2);
end

for tt = 1:fs*signalLength-(fs/2) %rain background sound
    for i = 1:32
        s3_ = s3{i};
        x3_(i) = a2(i)*s3_(tt);
    end
    y2_ = sum(x3_);
    y2(tt) = y2(tt) + y2_;
end
output(1:fs*signalLength-(fs/2)) = output(1:fs*signalLength-(fs/2)) + (y2*8);

soundsc(output,fs);plot(output)


