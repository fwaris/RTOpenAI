#!/usr/bin/env bash
set -euo pipefail

tag="144.7559.09"
asset_name="LiveKitWebRTC.xcframework.zip"
asset_url="https://github.com/livekit/webrtc-xcframework/releases/download/${tag}/${asset_name}"
expected_sha256="64da5637fbb171fb0bce7889e9e025ceb3521a1c9010bb5df47df9b54a659c30"

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
repo_root="$(cd "${script_dir}/.." && pwd)"
target="${repo_root}/src/Bindings/FsWebRTC.Bindings.Maui.iOS/LiveKitWebRTC.xcframework"
work_dir="${TMPDIR:-/tmp}/rtopenai-livekit-webrtc-${tag}"
zip_path="${work_dir}/${asset_name}"
unpack_dir="${work_dir}/unpacked"
framework_path="${unpack_dir}/LiveKitWebRTC.xcframework"

required_identifiers=(
    "ios-arm64"
    "ios-arm64_x86_64-simulator"
    "ios-arm64_x86_64-maccatalyst"
)

required_binaries=(
    "ios-arm64/LiveKitWebRTC.framework/LiveKitWebRTC"
    "ios-arm64_x86_64-simulator/LiveKitWebRTC.framework/LiveKitWebRTC"
    "ios-arm64_x86_64-maccatalyst/LiveKitWebRTC.framework/LiveKitWebRTC"
)

rm -rf "${work_dir}"
mkdir -p "${unpack_dir}"

curl -fL --retry 3 --retry-delay 2 -o "${zip_path}" "${asset_url}"

actual_sha256="$(shasum -a 256 "${zip_path}" | awk '{print $1}')"
if [[ "${actual_sha256}" != "${expected_sha256}" ]]; then
    echo "Unexpected ${asset_name} SHA256: ${actual_sha256}" >&2
    echo "Expected: ${expected_sha256}" >&2
    exit 1
fi

unzip -q "${zip_path}" -d "${unpack_dir}"

if [[ ! -d "${framework_path}" ]]; then
    echo "Missing unpacked framework: ${framework_path}" >&2
    exit 1
fi

for identifier in "${required_identifiers[@]}"; do
    if ! plutil -p "${framework_path}/Info.plist" | grep -q "\"${identifier}\""; then
        echo "Missing xcframework library identifier: ${identifier}" >&2
        exit 1
    fi
done

for binary in "${required_binaries[@]}"; do
    if [[ ! -f "${framework_path}/${binary}" ]]; then
        echo "Missing framework binary: ${binary}" >&2
        exit 1
    fi
done

rm -rf "${target}"
mkdir -p "${target}"
cp "${framework_path}/Info.plist" "${target}/Info.plist"
if [[ -f "${framework_path}/LICENSE" ]]; then
    cp "${framework_path}/LICENSE" "${target}/LICENSE"
fi

for identifier in "${required_identifiers[@]}"; do
    cp -R "${framework_path}/${identifier}" "${target}/${identifier}"
done

/usr/bin/python3 - "${target}/Info.plist" "${required_identifiers[@]}" <<'PY'
import plistlib
import sys
from pathlib import Path

plist_path = Path(sys.argv[1])
required = set(sys.argv[2:])

with plist_path.open("rb") as handle:
    data = plistlib.load(handle)

libraries = data.get("AvailableLibraries", [])
data["AvailableLibraries"] = [
    library for library in libraries
    if library.get("LibraryIdentifier") in required
]

found = {library.get("LibraryIdentifier") for library in data["AvailableLibraries"]}
missing = required - found
if missing:
    raise SystemExit(f"Pruned plist is missing required slices: {sorted(missing)}")

with plist_path.open("wb") as handle:
    plistlib.dump(data, handle, sort_keys=False)
PY

echo "Updated ${target} to LiveKitWebRTC ${tag}."
