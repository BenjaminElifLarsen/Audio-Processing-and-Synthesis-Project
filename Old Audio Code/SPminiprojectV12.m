%parametic equalisor, 
%
%create functions that calls these below 
%page 592 (604)
%% lp 
%{fpass, fstop, Apass, Astop}
clc
clear


%N = 6;
%Omega;
%Omega0;
fpass = 5000;
fstop = 6000;
Apass = 0.5;
Astop = 10;
%[y2,Fs] = audioread('test.mp3');
fs = 44100;
f = 22050;

WPass = (2*pi*fpass)/fs;
WStop = (2*pi*fstop)/fs;
OmegaPass = tan(WPass/2);
OmegaStop = tan(WStop/2);
Omega = 2*pi*f;
%Omega = 20000;

%%Apass  = 10*log10(1+(OmegaPass/Omega0)^(2*N));
%%Astop  = 10*log10(1+(OmegaStop/Omega0)^(2*N));

Epass = sqrt(10^(Apass/10)-1);
Estop = sqrt(10^(Astop/10)-1);




w = OmegaStop/OmegaPass;
e = Estop/Epass;
Nexact = log(e)/log(w);
N = ceil(Nexact);
Omega0 = OmegaPass/(Epass^(1/N));
MRLP = (1/(1+(Omega/Omega0)^(2*N)));
f0 = ((fs/pi)*atan(Omega0))/1000;
%% lp H(S)
j = sqrt(-1);
Sc = j*Omega; %S
H = 1+(-1)^N*(Sc/Omega0)^(2*N);
S2N = (-1)^(N-1)*Omega0^(2*N);

for i = 1:(2*N)
    theta(i) = (pi/(2*N))*(N-1+(2*i));
    S(i) = Omega0*exp((j*theta(i)));
    %theta(i)
end

if mod(N,2)==0
    k = N/2;
    for t = 1:k
          Dt(t+1) = -2*cos(theta(t))
    end
else 
    k = N/2;
    D(0+1) = (1+(Sc/Omega0));

    for t = 1:k

        Dt(t+1) = -2*cos(theta(t))

    end
end


if mod(N,2)==0
    
    H(0+1) = 1;
    for s = 1:(N/2)
        H(s+1) = 1/Dt(s+1);
    end
else      
    H(0+1) = (1+(Sc/Omega0));
    for s = 1:(N/2)
        H(s+1) = 1/Dt(s+1);
    end
end

%% lp H(Z)
Z = 1;

for i = 1:(N/2) 
   G(i+1) = Omega0^2/(1-2*Omega0*cos(theta(i))+Omega0^2);
   a(i+1,1) = (2*(Omega0^2-1))/(1-2*Omega0*cos(theta(i))+Omega0^2);
   a(i+1,2) = (1+2*Omega0*cos(theta(i))+Omega0^2)/(1-2*Omega0*cos(theta(i))+Omega0^2);
   HZ(i+1) = (G(i+1)*(1+Z^-1)^2)/(1+a(i+1,1)*Z^-1+a(i+1,2)*Z^-2); 
   %HZLPz = [G(i+1) 1 1 1 a(i+1,1) a(i+1,2)];
   %HZLP{i+1} = HZLPz;
   
end

if mod(N,2) == 0
    HZLP{0+1} = 1;
else
    G(0+1) = Omega0/(Omega0+1);
    a(0+1,1) = (Omega0-1)/(Omega0+1);
    HZ(0+1) = (G(0+1)*(1+Z^-1))/(1+a(0+1,1)*Z^-1);
    %HZLPz = [G(0+1) 1 1 1 a(0+1,1)];
    %HZLP{0+1} = HZLPz;
end

%%
% 
%  [H2,omegaGrid] = freqz(HZ);
%  y = abs(H2);
%  plot(omegaGrid,abs(H2));
%  xlabel('\omega in units of \pi');
%  ylabel('|H(\omega)|^2');
% 
% grid on;
% axis tight;
% calculate for each w 
% Hw = 1/(1+(tan(w/2)/Omega0)^(2*N));
% Hw = abs(Hw)^2;
% 

for i = 1:fs/2
    t(i) = 1/(1+(tan(pi*i/fs)/Omega0)^(2*N));
end
x = 1:fs/2;
%x = 0:(pi*f/20):2*pi;
plot(x,t/2)
axis tight;

%% Hp

%{fpass, fstop, Apass, Astop}
clc
clear

Z = 1;
%N = 6;
%Omega;
%Omega0;
fpass = 14000;
fstop = 13000;
Apass = 0.5;
Astop = 10;
%[y2,Fs] = audioread('test.mp3');
fs = 44100;
f = 22050;

%WPass = (2*pi*fpass)/fs;
%WStop = (2*pi*fstop)/fs;
%OmegaPass = tan(WPass/2);
%OmegaStop = tan(WStop/2);

w = (2*pi*f)/fs;
Omega = -cot(w/2);
Wpass = (2*pi*fpass)/fs;
Wstop = (2*pi*fstop)/fs;

OmegaPass = abs(cot((Wpass/2)));
OmegaStop = abs(cot((Wstop/2)));

Epass = sqrt(10^(Apass/10)-1);
Estop = sqrt(10^(Astop/10)-1);
s = [1+Z^-1 1-Z^-1];



%w = OmegaStop/OmegaPass;
e = Estop/Epass;
Nexact = log(e)/(log(OmegaStop/OmegaPass));
N = ceil(abs(Nexact));
Omega0 = OmegaPass/(Epass^(1/N));
MRHP = (1/(1+(Omega/Omega0)^(2*N)));
f0 = ((fs/pi)*atan(1/Omega0))/1000;
%% Hp H(S)
j = sqrt(-1);
Sc = j*Omega; %S
Omega0 = OmegaPass/(Epass)^(1/N);
H = 1+(-1)^N*(Sc/Omega0)^(2*N);
S2N = (-1)^(N-1)*Omega0^(2*N);

for i = 1:(2*N)
    theta(i) = (pi/(2*N))*(N-1+2*i);
    S(i) = Omega0*exp((j*theta(i)));
end

if mod(N,2)==0
    k = N/2;
    for t = 1:k
          Dt(t+1) = -2*cos(theta(t))
    end
else 
    k = N/2;
    Dt(0+1) = (1+(Omega0));

    for t = 1:k

        Dt(t+1) = -2*cos(theta(t))
        Dt
    end
end


if mod(N,2)==0
    H(0+1) = 1;
    for s = 1:(N/2)
        H(s+1) = 1/Dt(s+1);
    end
else      
    H(0+1) = (1+(Sc/Omega0));
    for s = 1:(N/2)
        H(s+1) = 1/Dt(s+1);
    end
end
%% Hp H(Z)
Z = 1;

for i = 1:(N/2) 
   G(i+1) = Omega0^2/(1-2*Omega0*cos(theta(i))+Omega0^2);
   a(i+1,1) = -(2*(Omega0^2-1))/(1-2*Omega0*cos(theta(i))+Omega0^2);
   a(i+1,2) = (1+2*Omega0*cos(theta(i))+Omega0^2)/(1-2*Omega0*cos(theta(i))+Omega0^2);
   HZHP(i+1) = (G(i+1)*(1+Z^-1)^2)/(1+a(i+1,1)*Z^-1+a(i+1,2)*Z^-2); %skriv ud 

   
   
end

if mod(N,2) == 0
    HZHP(0+1) = 1;
else
    G(0+1) = Omega0/(Omega0+1);
    a(0+1,1) = (Omega0-1)/(Omega0+1);
    HZHP(0+1) = (G(0+1)*(1+Z^-1))/(1+a(0+1,1)*Z^-1);
end

%%
% [H2HP,omegaGrid] = freqz(HZHP);
% y = abs(H2HP);
% plot(omegaGrid,abs(H2HP));
% xlabel('\omega in units of \pi');
% ylabel('|H(\omega)|^2');
% 
% grid on;
% axis tight;
% 
% Hw = 1/(1+(cot(w/2)/Omega0)^(2*N));
% %Hw = abs(Hw)^2;
wgrid=linspace(0,2*pi/2,1000);
for i = 1:length(wgrid)
    
    t(i) = 1/(1+(cot(wgrid(i)/2)/Omega0)^(2*N));
end
%x = 1:2*pi;

%x = 0:(pi*f/20):2*pi;
plot(wgrid,t)
axis tight;

%% BP

%{fpassSa,fpassSb, fstopPa, fpassPb, Apass, Astop}
clc
clear

Z = 1;
%N = 6;
%Omega;
%Omega0;
fpassPa = 2000;
fpassPb = 4000;
fstopSa = 1500;
fstopSb = 4500;

Apass = 0.5;
Astop = 10;
%[y2,Fs] = audioread('test.mp3');
fs = 20000;
f = 20000;

%WPass = (2*pi*fpass)/fs;
%WStop = (2*pi*fstop)/fs;
%OmegaPass = tan(WPass/2);
%OmegaStop = tan(WStop/2);

w = (2*pi*f)/fs;
 
%Wpass = (2*pi*fpass)/fs;
%Wstop = (2*pi*fstop)/fs;
Wpa = (2*pi*fpassPa)/fs;
Wpb = (2*pi*fpassPb)/fs;
Wsa = (2*pi*fstopSa)/fs;
Wsb = (2*pi*fstopSb)/fs;

c = sin(Wpa+Wpb)/(sin(Wpa)+sin(Wpb));


Omega = (c-cos(w))/sin(w);
OmegaPass = (c-cos(Wpb))/sin(Wpb);
OmegaSA = (c-cos(Wsa))/sin(Wsa);
OmegaSB = (c-cos(Wsb))/sin(Wsb);
OmegaStop = min(abs(OmegaSA),abs(OmegaSB));


Epass = sqrt(10^(Apass/10)-1);
Estop = sqrt(10^(Astop/10)-1);
s = [1+2*c*Z^-1+Z^-2 1-Z^-2];



%w = OmegaStop/OmegaPass;
e = Estop/Epass;
Nexact = log(e)/(log(OmegaStop/OmegaPass));
N = ceil(abs(Nexact));
Omega0 = OmegaPass/(Epass^(1/N));
MRBP = (1/(1+(Omega/Omega0)^(2*N)));
f0 = ((fs/pi)*atan(1/Omega0))/1000;
%% BP H(S)
j = sqrt(-1);
Sc = j*Omega; %S
Omega0 = OmegaPass/(Epass)^(1/N);
H = 1+(-1)^N*(Sc/Omega0)^(2*N);
S2N = (-1)^(N-1)*Omega0^(2*N);

for i = 1:(2*N)
    theta(i) = (pi/(2*N))*(N-1+2*i);
    S(i) = Omega0*exp((j*theta(i)));
end

if mod(N,2)==0
    k = N/2;
    for t = 1:k
          Dt(t+1) = -2*cos(theta(t))
    end
else 
    k = N/2;
    Dt(0+1) = (1+(Sc/Omega0));

    for t = 1:k

        Dt(t+1) = -2*cos(theta(t))

    end
end


if mod(N,2)==0
    H(0+1) = 1;
    for s = 1:(N/2)
        H(s+1) = 1/Dt(s+1);
    end
else      
    H(0+1) = (1+(Sc/Omega0));
    for s = 1:(N/2)
        H(s+1) = 1/Dt(s+1);
    end
end
%% BP H(Z)
Z = 1;

for i = 1:(N/2) 
   G(i+1) = Omega0^2/(1-2*Omega0*cos(Dt(i))+Omega0^2);
   a(i+1,1) = (4*c*(Omega0*cos(Dt(i))-1))/(1-2*Omega0*cos(Dt(i))+Omega0^2);
   a(i+1,2) = (2*(2*c^2+1-Omega0^2))/(1-2*Omega0*cos(Dt(i))+Omega0^2);
   a(i+1,3) = (4*c*(Omega0*cos(Dt(i))+1))/(1-2*Omega0*cos(Dt(i))+Omega0^2);
   a(i+1,4) = (1+2*Omega0*cos(Dt(i))+Omega0^2)/(1-2*Omega0*cos(Dt(i))+Omega0^2);
   HZBP(i+1) = (G(i+1)*(1+Z^-2)^2)/(1+a(i+1,1)*Z^+1+a(i+1,2)*Z^-2+a(i+1,3)*Z^-3+a(i+1,4)*Z^-4); 
end

if mod(N,2) == 0
    HZBP(0+1) = 1;
else
    G(0+1) = Omega0/(Omega0+1);
    a(0+1,1) = (2*c)/(1+Omega0);
    a(0+1,2) = (1-Omega0)/(1+Omega0);
    HZBP(0+1) = (G(0+1)*(1-Z^-2))/(1+a(0+1,1)*Z^-1+a(0+2)*Z^-2);
end

%%
for i = 1:fs
    t(i) = 1/(1+((c-cos(w))/(Omega0*sin(w)))^(N*2));
end
x = 1:fs;
%x = 0:(pi*f/20):2*pi;
plot(x,t)
axis tight;

%% BS

%{fpassSa,fpassSb, fstopPa, fpassPb, Apass, Astop}
clc
clear

Z = 1;
%N = 6;
%Omega;
%Omega0;
fpassPa = 1500;
fpassPb = 4500;
fstopSa = 2000;
fstopSb = 4000;

Apass = 0.5;
Astop = 10;
%[y2,Fs] = audioread('test.mp3');
fs = 20000;
f = 20000;

%WPass = (2*pi*fpass)/fs;
%WStop = (2*pi*fstop)/fs;
%OmegaPass = tan(WPass/2);
%OmegaStop = tan(WStop/2);

w = (2*pi*f)/fs;
 
%Wpass = (2*pi*fpass)/fs;
%Wstop = (2*pi*fstop)/fs;
Wpa = (2*pi*fpassPa)/fs;
Wpb = (2*pi*fpassPb)/fs;
Wsa = (2*pi*fstopSa)/fs;
Wsb = (2*pi*fstopSb)/fs;

c = sin(Wpa+Wpb)/(sin(Wpa)+sin(Wpb));


Omega = sin(w)/(cos(w)-c);
OmegaPass = abs(sin(Wpb)/(cos(Wpb)-c));
OmegaSA = sin(Wsa)/(cos(Wsa)-c);
OmegaSB = sin(Wsb)/(cos(Wsb)-c);
OmegaStop = min(abs(OmegaSA),abs(OmegaSB));


Epass = sqrt(10^(Apass/10)-1);
Estop = sqrt(10^(Astop/10)-1);
s = [1-Z^-2 1-2*c*Z^-1+Z-2];



%w = OmegaStop/OmegaPass;
e = Estop/Epass;
Nexact = log(e)/(log(OmegaStop/OmegaPass));
N = ceil(abs(Nexact));
Omega0 = OmegaPass/(Epass^(1/N));
MRBS = (1/(1+(Omega/Omega0)^(2*N)));
f0 = ((fs/pi)*atan(1/Omega0))/1000;
%% BS H(S)
j = sqrt(-1);
Sc = j*Omega; %S
Omega0 = OmegaPass/(Epass)^(1/N);
H = 1+(-1)^N*(Sc/Omega0)^(2*N);
S2N = (-1)^(N-1)*Omega0^(2*N);

for i = 1:(2*N)
    theta(i) = (pi/(2*N))*(N-1+2*i);
    S(i) = Omega0*exp((j*theta(i)));
end

if mod(N,2)==0
    k = N/2;
    for t = 1:k
          Dt(t+1) = -2*cos(theta(t))
    end
else 
    k = N/2;
    D(0+1) = (1+(Sc/Omega0));

    for t = 1:k

        Dt(t+1) = -2*cos(theta(t))

    end
end


if mod(N,2)==0
    H(0+1) = 1;
    for s = 1:(N/2)
        H(s+1) = 1/Dt(s+1);
    end
else      
    H(0+1) = (1+(Sc/Omega0));
    for s = 1:(N/2)
        H(s+1) = 1/Dt(s+1);
    end
end
%% BS H(Z)
Z = 1;

for i = 1:(N/2) 
   G(i+1) = Omega0^2/(1-2*Omega0*cos(theta(i))+Omega0^2);
   a(i+1,1) = (4*c*Omega0*(cos(theta(i))-Omega0))/(1-2*Omega0*cos(theta(i))+Omega0^2);
   a(i+1,2) = (2*(2*c^2*Omega0^2+Omega0^2-1))/(1-2*Omega0*cos(theta(i))+Omega0^2);
   a(i+1,3) = -(4*c*Omega0*(cos(theta(i))+Omega0))/(1-2*Omega0*cos(theta(i))+Omega0^2);
   a(i+1,4) = (1+2*Omega0*cos(theta(i))+Omega0^2)/(1-2*Omega0*cos(theta(i))+Omega0^2);
   HZBS(i+1) = (G(i+1)*(1-2*c*Z^-1+Z^-2)^2)/(1+a(i+1,1)*Z^-1+a(i+1,2)*Z^-2+a(i+1,3)*Z^-3+a(i+1,4)*Z^-4); 

   %multiple the different Hi(Z) together and replace Z with jomega 
   %multipe all Hi(z) together and repalce z with jomega 
end

if mod(N,2) == 0
    HZBS(0+1) = 1;
else
    G(0+1) = Omega0/(Omega0+1);
    a(0+1,1) = (2*c*Omega0)/(1+Omega0);
    a(0+1,2) = (1-Omega0)/(1+Omega0);
    HZBS(0+1) = (G(0+1)*(1-2*c*Z^-1+Z^-2))/(1+a(0+1,1)*Z^-1+a(0+1,2)*Z^-2);
end

%%

[HZBS2,omegaGrid] = freqz(HZBS);
y = abs(HZBS2);
plot(omegaGrid,abs(HZBS2));
xlabel('\omega in units of \pi');
ylabel('|H(\omega)|^2');

grid on;
axis tight;

Hw = 1/(1+(cot(w/2)/Omega0)^(2*N));
Hw = abs(Hw)^2;


%% test
test = [1 2];
test2 = [3 4];
testA{1} = test;
testA{2} = test2;
[testr] = testA{1};
testr
testd = testr*2
conv(testr,testd)


