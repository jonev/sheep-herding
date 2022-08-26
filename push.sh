echo "Building app"
docker build -f ./Api/Dockerfile -t jonev/home-hosting:masters .
docker push jonev/home-hosting:masters
ssh -t jone@prod1 '/home/jone/repos/home-hosting/masters/deploy.sh'
echo "Successfully deployed the app"