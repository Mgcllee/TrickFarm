cmake_minimum_required(VERSION 3.8)

file(GLOB SOURCES "*.h")
file(GLOB SOURCES "*.cpp")

project (TrickFarmEPollServer-cmake)
add_executable(TrickFarmEPollServer-cmake TrickFarmEPollServer.cpp)