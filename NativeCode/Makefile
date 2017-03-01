SRCS=AudioPluginUtil.cpp \
Plugin_Helm.cpp \
OBJS=$(SRCS:.cpp=*.o)
OUTPUT=libAudioPluginHelm.so
CXXFLAGS=-I. -O2 -fPIC
LDFLAGS=-shared -rdynamic -fPIC
CXX=g++

all: $(OUTPUT)
clean:
	rm -f $(OUTPUT) $(OBJS)

$(OUTPUT): $(OBJS)
	$(CXX) $(LDFLAGS) -o $(OUTPUT) $(OBJS)

.cpp.o:
	$(CXX) $(CXXFLAGS) -c $<
