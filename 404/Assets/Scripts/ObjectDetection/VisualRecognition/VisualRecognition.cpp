#include "VisualRecognition.h"
#include <opencv2/opencv.hpp>

extern "C"
{
    // The recognition model, created during prepareModel
    cv::dnn::Net net;

    // The details of detected matches from the last round
    // of doRecognise
    cv::Mat detections;

    void prepareModel(char *dirname)
    {
        const char *protoFile = "MobileNetSSD_deploy.prototxt";
        const char *modelFile = "MobileNetSSD_deploy.caffemodel";

        net = cv::dnn::readNetFromCaffe(std::string(dirname) + std::string("/") + std::string(protoFile), std::string(dirname) + std::string("/") +
                                                                                                              std::string(modelFile));

        std::vector<cv::String> n = net.getLayerNames();
    }

    int doRecognise(char *imageData, int width, int height)
    {
        std::string CLASSES[] = {"background", "aeroplane", "bicycle", "bird", "boat",
                                 "bottle", "bus", "car", "cat", "chair", "cow", "diningtable",
                                 "dog", "horse", "motorbike", "person", "pottedplant", "sheep",
                                 "sofa", "train", "tvmonitor"};

        cv::Mat rawimage = cv::Mat(height, width, CV_8UC4, imageData);
        cv::Mat image;
        cv::cvtColor(rawimage, image, cv::COLOR_RGBA2BGR, 3);
        cv::flip(image, image, 0);
        int h = image.rows;
        int w = image.cols;

        cv::Mat resized;
        cv::resize(image, resized, cv::Size(300, 300));

        cv::Mat blob = cv::dnn::blobFromImage(resized, 0.007843, cv::Size(300, 300), cv::Scalar(127.5));

        net.setInput(blob);
        detections = net.forward();

        return detections.size[2];
    }

    void retrieveMatch(int i, int &category, float &confidence, float &sx, float &sy, float &ex, float &ey)
    {
        int pos[4] = {0, 0, 0, 0};
        pos[2] = i;

        pos[3] = 1;
        category = detections.at<float>(pos);
        pos[3] = 2;
        confidence = detections.at<float>(pos);
        pos[3] = 3;
        sx = detections.at<float>(pos);
        pos[3] = 4;
        sy = detections.at<float>(pos);
        pos[3] = 5;
        ex = detections.at<float>(pos);
        pos[3] = 6;
        ey = detections.at<float>(pos);
    }
}