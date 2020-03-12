classdef miniprojectSPHP < audioPlugin & matlab.System
    %UNTITLED2 Summary of this class goes here
    %   Detailed explanation goes here
    %audioTestBench
    
    properties
        
        SamplesPerFrame = 1024;
        SampleRate = 44100;
        
    end
    
    properties (Access = private)
        %SampleRate;
        
    end
    
    methods (Access = protected)
        
        function setupImpl(obj, u)
            global LpN;
            

            
            %% hp
            global HpN;
                        fs = obj.SampleRate;
            f = fs/2;
            Hpw = (2*pi*f)/fs;
            HpOmega = -cot(Hpw/2);
            Hpfpass = 4000;
            Hpfstop = 5000;
            HpApass = 0.5;
            HpAstop = 10;
            HpWpass = (2*pi*Hpfpass)/fs;
            HpWstop = (2*pi*Hpfstop)/fs;
            
            HpOmegaPass = abs(cot((HpWpass/2)));
            HpOmegaStop = abs(cot((HpWstop/2)));
            
            HpEpass = sqrt(10^(HpApass/10)-1);
            HpEstop = sqrt(10^(HpAstop/10)-1);
            Hpe = HpEstop/HpEpass;
            HpNexact = log(Hpe)/(log(HpOmegaStop/HpOmegaPass));
            HpN = ceil(abs(HpNexact));
            HpOmega0 = HpOmegaPass/(HpEpass^(1/HpN));
            Hpj = sqrt(-1);
            HpSc = Hpj*HpOmega; %S
            
            
            for i = 1:(2*HpN)
                Hptheta(i) = (pi/(2*HpN))*(HpN-1+(2*i));
                HpS(i) = HpOmega0*exp((Hpj*Hptheta(i)));
                
            end
            
            if mod(HpN,2)==0
                k = HpN/2;
                for t = 1:k
                    HpthetaD(t+1) = -2*cos(Hptheta(t));
                end
            else
                k = HpN/2;
                HpthetaD(0+1) = (1+(HpOmega0));
                
                for t = 1:k
                    
                    HpthetaD(t+1) = -2*cos(Hptheta(t));
                    
                end
            end
            global HpG;
            global Hpa;
            for i = 1:(HpN/2)
                HpG(i+1) = HpOmega0^2/(1-2*HpOmega0*cos(Hptheta(i))+HpOmega0^2);
                Hpa(i+1,1) = -(2*(HpOmega0^2-1))/(1-2*HpOmega0*cos(Hptheta(i))+HpOmega0^2);
                Hpa(i+1,2) = (1+2*HpOmega0*cos(Hptheta(i))+HpOmega0^2)/(1-2*HpOmega0*cos(Hptheta(i))+HpOmega0^2);
                
                %HpHZHP(i+1) = (HpG(i+1)*(1+Z^-1)^2)/(1+Hpa(i+1,1)*Z^-1+Hpa(i+1,2)*Z^-2); %skriv ud
            end
            
            if mod(HpN,2) == 0
                %HpHZHP(0+1) = 1;
                HpG(1) = 0;
                Hpa(1,1) = 0;
            else
                HpG(0+1) = HpOmega0/(HpOmega0+1);
                Hpa(0+1,1) = (HpOmega0-1)/(HpOmega0+1);
                %HpHZHP(0+1) = (HpG(0+1)*(1+Z^-1))/(1+Hpa(0+1,1)*Z^-1);
            end
            
 
            
        end
        
        
        %%
        function y = stepImpl(obj, x)
            obj.SamplesPerFrame=length(x);
            obj.SampleRate=getSampleRate(obj);
            
            %%
            global LpN;
            global LpG;
            global Lpa;
            global HpN;
            global HpG;
            global Hpa;
            global BPN;
            %global BPG;
            %global BPa;
            global BSN;
            global BSG;
            global BSa;

%             
                hpy = zeros(size(x));
            %yhp = zeros(size(ylp));
            for ti = 1:HpN/2+1
                hpw(ti,1) = 0;
                hpw(ti,2) = 0;
                hpw(ti,3) = 0;
                hpw(ti,4) = 0;
                hpw(ti,5) = 0;
                
            end
            for t = 1:length(x)
                x2(1,:) = x(t,:);
                k = HpN/2;
                if mod(HpN,2) == 0

                    hpy(1,1:2) = x2(1);
                    x2(1+1,:) = hpy(1,:);
                else
                    hpw(1,1) = x2(1)-Hpa(1,1)*hpw(1,1);
                    hpy(1,1:2) = HpG(1)*hpw(1,1);
                    x2(1+1,:) = hpy(1,:);
                end
                for i = 2:1:k+1
                    hpw(i,1) = x2(i)-Hpa(i,1)*hpw(i,2)-Hpa(i,2)*hpw(i,3);
                    hpy(i,:) = HpG(i)*hpw(i,1)-2*(HpG(i)*hpw(i,2))+HpG(i)*hpw(i,3);
                    hpw(i,3) = hpw(i,2);
                    hpw(i,2) = hpw(i,1);
                    i;
                    x2(i+1,:) = hpy(i,:);
                    
                    
                end
                %yhp(t,:) = hy(floor(4),:);

                y(t,:) = hpy(floor(k+1),:);
            end
            y;
%             

            
        end
    end
end


